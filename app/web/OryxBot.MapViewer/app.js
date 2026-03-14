// Map configuration matching albionfreemarket's coordinate system
const CONFIG = {
    map: {
        tilePath: 'data/tiles/maps/{z}/map_{x}_{y}.webp',
        options: {
            crs: L.CRS.Simple,
            minZoom: 0,
            maxZoom: 7,
            zoomAnimationThreshold: 0
        },
        view: { center: [-180, 130], zoom: 1 },
        tileLayer: {
            tileSize: 256,
            noWrap: true,
            maxNativeZoom: 6,
            minZoom: 0,
            zoomOffset: 0
        }
    },
    coordinates: {
        xOffset: 128,
        yOffset: -128,
        scaleFactor: 200,
        gameScale: { sourceRange: 800, targetRange: 256 }
    },
    minZoomForOverlays: 5,
    maxConcurrentLoads: 8,
    viewportPadding: 0.2
};

// PvP category colors
const PVP_COLORS = {
    blue:   '#2196F3',
    yellow: '#FFC107',
    red:    '#f44336',
    black:  '#9C27B0',
    other:  '#888888'
};

// Coordinate transformer
const CoordTransform = {
    gameToMap(val) {
        return val / CONFIG.coordinates.gameScale.sourceRange * CONFIG.coordinates.gameScale.targetRange;
    },
    applyOffsets([x, y]) {
        return [x + CONFIG.coordinates.xOffset, y + CONFIG.coordinates.yOffset];
    },
    worldToMapCenter(worldmapposition) {
        if (!worldmapposition) return null;
        const mx = this.gameToMap(worldmapposition[0]);
        const my = this.gameToMap(worldmapposition[1]);
        const [x, y] = this.applyOffsets([mx, my]);
        return { x, y };
    }
};

let map = null;
let markersLayer = null;
let edgesLayer = null;
let overlaysLayer = null;
let graphData = null;
let allClusters = [];      // marker objects
let clusterDataMap = {};   // id -> { cluster, loc, mapCenter, marker }
let activeOverlays = new Map(); // id -> imageOverlay
let loadingOverlays = new Set();

function getPvpColor(loc, cluster) {
    const pvp = (loc && loc.pvpCategory) || '';
    return PVP_COLORS[pvp] || PVP_COLORS.other;
}

document.addEventListener('DOMContentLoaded', () => initApp());

async function initApp() {
    try {
        const [graphResp, locResp] = await Promise.all([
            fetch('data/world-graph.json?v=3'),
            fetch('data/albionLocations.json').catch(() => null)
        ]);

        if (!graphResp.ok) throw new Error(`Failed to load data: ${graphResp.statusText}`);
        graphData = await graphResp.json();

        let locations = null;
        if (locResp && locResp.ok) {
            locations = await locResp.json();
        }

        document.getElementById('loading').style.display = 'none';
        initMap();
        processData(graphData, locations);
        setupEventListeners();
    } catch (error) {
        document.getElementById('loading').innerText = 'Error loading data: ' + error.message;
        console.error(error);
    }
}

function initMap() {
    map = L.map('map', {
        ...CONFIG.map.options,
        attributionControl: false,
        zoomAnimation: true,
        fadeAnimation: true,
        markerZoomAnimation: true
    }).setView(CONFIG.map.view.center, CONFIG.map.view.zoom);

    L.tileLayer(CONFIG.map.tilePath, CONFIG.map.tileLayer).addTo(map);

    edgesLayer = L.layerGroup().addTo(map);
    overlaysLayer = L.layerGroup().addTo(map);
    markersLayer = L.layerGroup().addTo(map);

    map.on('zoomend moveend', () => {
        updateMarkerSizes();
        updateOverlays();
    });
}

function processData(data, locations) {
    if (!data.clusters) return;

    const locLookup = new Map();
    if (locations) {
        locations.forEach(loc => {
            if (loc.id) locLookup.set(loc.id, loc);
        });
    }

    Object.values(data.clusters).forEach(c => {
        let mapCenter = null;
        const loc = locLookup.get(c.id);

        if (loc && loc.worldmapposition) {
            const wmp = Array.isArray(loc.worldmapposition)
                ? loc.worldmapposition
                : [loc.worldmapposition.x, loc.worldmapposition.y];
            mapCenter = CoordTransform.worldToMapCenter(wmp);
        }

        if (!mapCenter && c.worldX !== undefined && c.worldY !== undefined) {
            mapCenter = CoordTransform.worldToMapCenter([c.worldX, c.worldY]);
        }

        if (!mapCenter) return;

        const color = getPvpColor(loc, c);
        const displayName = c.displayName || (loc && loc.displayName) || c.id;
        const tierStr = c.tier || (loc && loc.tier) || '?';

        const marker = L.circleMarker([mapCenter.y, mapCenter.x], {
            radius: 4,
            color: '#000',
            weight: 1,
            opacity: 1,
            fillColor: color,
            fillOpacity: 0.9
        });

        marker.bindTooltip(
            `<b>${displayName}</b><br>T${tierStr} | ${(loc && loc.pvpCategory) || c.zone || '?'}`,
            { direction: 'top', offset: [0, -5] }
        );

        marker.on('click', () => showClusterInfo(c, loc));

        marker._clusterData = c;
        marker._locData = loc;
        marker._mapCenter = mapCenter;
        marker._displayName = displayName;

        allClusters.push(marker);
        markersLayer.addLayer(marker);

        clusterDataMap[c.id] = { cluster: c, loc, mapCenter, marker };
    });

    // Draw edges
    if (data.edges) {
        data.edges.forEach(e => {
            const from = clusterDataMap[e.fromClusterId];
            const to = clusterDataMap[e.toClusterId];
            if (from && to) {
                const line = L.polyline(
                    [[from.mapCenter.y, from.mapCenter.x],
                     [to.mapCenter.y, to.mapCenter.x]],
                    { color: '#555', weight: 1, opacity: 0.4 }
                );
                edgesLayer.addLayer(line);
            }
        });
    }

    updateMarkerSizes();
}

function updateMarkerSizes() {
    if (!map) return;
    const zoom = map.getZoom();
    const showOverlays = zoom >= CONFIG.minZoomForOverlays;

    // Scale markers, hide them when overlays are visible
    const radius = zoom < 2 ? 3 : zoom < 4 ? 5 : zoom < 6 ? 7 : 10;
    allClusters.forEach(m => {
        m.setRadius(radius);
        // When overlays are active and this cluster has one loaded, reduce marker opacity
        if (showOverlays && activeOverlays.has(m._clusterData.id)) {
            m.setStyle({ fillOpacity: 0, opacity: 0 });
        } else {
            m.setStyle({ fillOpacity: 0.9, opacity: 1 });
        }
    });

    // Show/hide edges
    if (zoom >= 3) {
        if (!map.hasLayer(edgesLayer)) map.addLayer(edgesLayer);
    } else {
        if (map.hasLayer(edgesLayer)) map.removeLayer(edgesLayer);
    }
}

function updateOverlays() {
    if (!map) return;
    const zoom = map.getZoom();

    if (zoom < CONFIG.minZoomForOverlays) {
        // Remove all overlays when zoomed out
        overlaysLayer.clearLayers();
        activeOverlays.clear();
        // Restore marker visibility
        allClusters.forEach(m => m.setStyle({ fillOpacity: 0.9, opacity: 1 }));
        return;
    }

    const bounds = map.getBounds().pad(CONFIG.viewportPadding);

    // Remove overlays outside viewport
    for (const [id, overlay] of activeOverlays) {
        const data = clusterDataMap[id];
        if (!data || !bounds.contains(L.latLng(data.mapCenter.y, data.mapCenter.x))) {
            overlaysLayer.removeLayer(overlay);
            activeOverlays.delete(id);
            // Restore marker
            if (data && data.marker) {
                data.marker.setStyle({ fillOpacity: 0.9, opacity: 1 });
            }
        }
    }

    // Load overlays for visible clusters
    const visible = allClusters.filter(m => {
        if (!m._locData || !m._locData.imageFile) return false;
        if (activeOverlays.has(m._clusterData.id)) return false;
        if (loadingOverlays.has(m._clusterData.id)) return false;
        return bounds.contains(L.latLng(m._mapCenter.y, m._mapCenter.x));
    });

    // Limit concurrent loads
    const toLoad = visible.slice(0, CONFIG.maxConcurrentLoads - loadingOverlays.size);
    toLoad.forEach(m => loadOverlay(m));
}

function loadOverlay(marker) {
    const loc = marker._locData;
    const id = marker._clusterData.id;
    const mapCenter = marker._mapCenter;

    if (!loc || !loc.imageFile) return;

    loadingOverlays.add(id);

    const webpName = loc.imageFile.replace('.png', '.webp');
    const imgUrl = `data/tiles/images/${webpName}`;

    const img = new Image();
    img.onload = () => {
        loadingOverlays.delete(id);

        // Check we're still at the right zoom
        if (map.getZoom() < CONFIG.minZoomForOverlays) return;

        const w = img.naturalWidth / CONFIG.coordinates.scaleFactor;
        const h = img.naturalHeight / CONFIG.coordinates.scaleFactor;

        const overlayBounds = [
            [mapCenter.y - h / 2, mapCenter.x - w / 2],
            [mapCenter.y + h / 2, mapCenter.x + w / 2]
        ];

        const imageOverlay = L.imageOverlay(imgUrl, overlayBounds, {
            interactive: true,
            attribution: loc.displayName
        });

        imageOverlay.on('click', () => showClusterInfo(marker._clusterData, loc));

        // Add label on top of overlay
        const labelGroup = L.layerGroup();
        labelGroup.addLayer(imageOverlay);

        const pvpClass = `pvp-${loc.pvpCategory || 'other'}`;
        const topLabel = L.marker([mapCenter.y + h / 2, mapCenter.x], {
            icon: L.divIcon({
                className: `map-label ${pvpClass}`,
                html: `<div>${loc.displayName || id}</div>`,
                iconSize: null
            }),
            interactive: false
        });
        labelGroup.addLayer(topLabel);

        const tierLabel = L.marker([mapCenter.y - h / 2, mapCenter.x], {
            icon: L.divIcon({
                className: `map-label small ${pvpClass}`,
                html: `<div>T${loc.tier || '?'}${loc.quality ? ' Q' + loc.quality : ''}</div>`,
                iconSize: null
            }),
            interactive: false
        });
        labelGroup.addLayer(tierLabel);

        overlaysLayer.addLayer(labelGroup);
        activeOverlays.set(id, labelGroup);

        // Hide the circle marker
        marker.setStyle({ fillOpacity: 0, opacity: 0 });
    };

    img.onerror = () => {
        loadingOverlays.delete(id);
    };

    img.src = imgUrl;
}

function showClusterInfo(cluster, loc) {
    const panel = document.getElementById('infoPanel');
    const details = document.getElementById('clusterDetails');

    const pvp = (loc && loc.pvpCategory) || cluster.zone || 'N/A';
    const color = PVP_COLORS[pvp] || PVP_COLORS.other;

    panel.classList.remove('hidden');
    details.innerHTML = `
        <p><strong>ID:</strong> ${cluster.id}</p>
        <p><strong>Name:</strong> ${cluster.displayName || (loc && loc.displayName) || 'Unknown'}</p>
        <p><strong>Type:</strong> ${cluster.type || (loc && loc.type) || 'N/A'}</p>
        <p><strong>Tier:</strong> ${cluster.tier || (loc && loc.tier) || 'N/A'}</p>
        <p><strong>Biome:</strong> ${cluster.biome || (loc && loc.biome) || 'N/A'}</p>
        <p><strong>PvP Zone:</strong> <span style="color:${color};font-weight:600">${pvp}</span></p>
        ${loc ? `<p><strong>Category:</strong> ${loc.mapCategory || 'N/A'}</p>` : ''}
    `;
}

function setupEventListeners() {
    const searchInput = document.getElementById('searchInput');
    document.getElementById('searchBtn').addEventListener('click', doSearch);
    searchInput.addEventListener('keydown', e => {
        if (e.key === 'Enter') doSearch();
    });

    ['filterWorld', 'filterCity', 'filterDungeon'].forEach(id => {
        document.getElementById(id).addEventListener('change', updateFilters);
    });
}

function doSearch() {
    const term = document.getElementById('searchInput').value.toLowerCase().trim();
    if (!term) return;

    const match = allClusters.find(m =>
        m._clusterData.id.toLowerCase() === term ||
        m._displayName.toLowerCase().includes(term)
    );

    if (match) {
        map.setView([match._mapCenter.y, match._mapCenter.x], 5, {
            animate: true,
            duration: 1
        });
        match.openTooltip();
        showClusterInfo(match._clusterData, match._locData);
    }
}

function updateFilters() {
    const showWorld = document.getElementById('filterWorld').checked;
    const showCity = document.getElementById('filterCity').checked;
    const showDungeon = document.getElementById('filterDungeon').checked;

    allClusters.forEach(m => {
        const type = m._clusterData.type;
        let visible = true;
        if (type === 'WORLD' && !showWorld) visible = false;
        if (type === 'CITY' && !showCity) visible = false;
        if (type === 'DUNGEON' && !showDungeon) visible = false;

        if (visible && !markersLayer.hasLayer(m)) {
            markersLayer.addLayer(m);
        } else if (!visible && markersLayer.hasLayer(m)) {
            markersLayer.removeLayer(m);
        }
    });
}
