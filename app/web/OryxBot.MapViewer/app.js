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
        gameScale: { sourceRange: 800, targetRange: 256 },
        rotation: { angle: -45, scaleAfterRotation: 0.7 },
        exits: { radiusSmall: 5, radiusLarge: 10 }
    },
    minZoomForOverlays: 4,
    maxConcurrentLoads: 20,
    viewportPadding: 0.3
};

// PvP category colors (matching albionfreemarket)
const PVP_COLORS = {
    blue:   '#3a86ff',
    yellow: '#ffd60a',
    red:    '#e63946',
    black:  '#2d2d2d',
    white:  '#ffffff',
    green:  '#2a9d8f',
    other:  '#888888'
};

// mapCategory -> pvp color key (for exit markers)
const MAP_CATEGORY_PVP = {
    arena: 'blue', corrupted: 'blue', dungeon: 'red', island: 'blue',
    expedition: 'blue', hideout: 'blue', portalcity: 'white', hellden: 'blue',
    startingcity: 'white', rest: 'white', city: 'white', startarea: 'blue',
    debug_black: 'blue', openworld: 'blue', passage: 'green', roads: 'blue', other: 'blue'
};

// Coordinate transformer (matches albionfreemarket's CoordinatesTransformer)
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
    },
    rotateAndScale([x, y]) {
        const angle = CONFIG.coordinates.rotation.angle * Math.PI / 180;
        const cos = Math.cos(angle);
        const sin = Math.sin(angle);
        const s = CONFIG.coordinates.rotation.scaleAfterRotation;
        return [(x * cos - y * sin) * s, (x * sin + y * cos) * s];
    },
    // Convert a local minimap position to Leaflet map coordinates
    // pos: {x, y} position on the minimap
    // loc: location object with minimapBoundsMin/Max and mapCenter
    // overlayW, overlayH: overlay dimensions on the map (img.naturalWidth/Height / scaleFactor)
    localToMapCoords(pos, loc, overlayW, overlayH) {
        if (!loc.mapCenter) return null;
        const bMin = loc._boundsMin;
        const bMax = loc._boundsMax;
        if (!bMin || !bMax) return null;

        const cx = (bMin.x + bMax.x) / 2;
        const cy = (bMin.y + bMax.y) / 2;

        let dx = pos.x - cx;
        let dy = pos.y - cy;
        [dx, dy] = this.rotateAndScale([dx, dy]);

        const normX = (dx + cx - bMin.x) / (bMax.x - bMin.x);
        const normY = (dy + cy - bMin.y) / (bMax.y - bMin.y);

        const lat = loc.mapCenter.y - overlayH / 2 + normY * overlayH;
        const lng = loc.mapCenter.x - overlayW / 2 + normX * overlayW;
        return [lat, lng];
    }
};

// OryxBot icon as data URI (favicon + pin stem with subtle dark outline)
const ORYXBOT_ICON_SVG = `<svg xmlns="http://www.w3.org/2000/svg" width="32" height="44" viewBox="0 0 32 44">
  <line x1="16" y1="30" x2="16" y2="44" stroke="#1a1b17" stroke-width="4" stroke-linecap="round"/>
  <line x1="16" y1="30" x2="16" y2="44" stroke="#d3c8a8" stroke-width="2" stroke-linecap="round"/>
  <circle cx="16" cy="30" r="4" fill="#1a1b17"/>
  <circle cx="16" cy="30" r="2.5" fill="#d3c8a8"/>
  <svg x="2" y="0" width="28" height="28" viewBox="0 0 700 654">
    <g transform="translate(0,654) scale(0.1,-0.1)" fill="#d3c8a8" stroke="#1a1b17" stroke-width="120">
      <path d="M2380 6331 c-41 -5 -95 -12 -120 -16 l-45 -8 95 -24 c589 -148 1015 -364 1300 -659 105 -108 156 -173 225 -288 39 -64 54 -80 108 -110 306 -172 452 -498 417 -931 -14 -168 -42 -305 -62 -305 -8 0 -9 26 -4 88 14 183 -27 461 -94 641 -55 149 -145 292 -243 384 -42 39 -57 45 -57 24 0 -32 -228 -616 -281 -718 -62 -119 -155 -213 -279 -281 -73 -40 -165 -74 -324 -118 -405 -113 -699 -281 -978 -556 -179 -177 -306 -352 -413 -569 -146 -297 -205 -554 -205 -892 0 -114 -4 -183 -10 -183 -13 0 -50 60 -118 193 -328 640 -389 1430 -166 2130 47 148 134 343 217 490 358 630 974 1070 1632 1166 l120 18 -104 52 c-174 88 -800 212 -561 213 -56 1 -366 -153 -551 -274 -673 -440 -1138 -1134 -1293 -1933 -42 -214 -50 -316 -50 -595 0 -301 12 -423 64 -672 123 -583 393 -1095 799 -1510 393 -404 806 -653 1311 -792 463 -128 997 -138 1465 -27 1091 259 1937 1109 2214 2226 64 256 91 488 91 775 0 495 -101 933 -314 1360 -161 322 -343 569 -608 826 -396 381 -893 646 -1428 759 -103 21 -414 65 -459 65 -9 0 29 -24 84 -53 109 -58 158 -89 280 -181 89 -67 271 -241 325 -310 25 -32 51 -51 90 -67 458 -188 856 -514 1134 -928 448 -668 560 -1555 296 -2351 -178 -539 -492 -977 -932 -1301 -102 -75 -271 -179 -291 -179 -7 0 -42 26 -77 59 -98 88 -200 150 -465 283 -389 196 -515 270 -598 351 -68 67 -87 108 -87 189 0 50 7 77 31 128 61 130 149 191 289 198 213 11 397 -89 660 -357 126 -128 156 -153 226 -189 106 -54 178 -72 289 -72 200 0 355 81 458 238 54 83 74 137 86 227 23 185 -40 347 -194 800 -213 212 -300 341 -345 511 -24 94 -27 253 -6 335 67 257 58 445 -31 627 -25 50 -49 92 -54 92 -5 0 -16 -15 -25 -32 -24 -46 -74 -118 -82 -118 -4 0 -7 125 -7 278 0 222 -4 299 -18 385 -166 986 -796 1627 -1742 1772 -115 18 -474 27 -585 16z m2065 -2991 c22 -16 60 -49 84 -75 l43 -47 -6 -81 c-6 -71 -4 -87 14 -122 11 -21 20 -41 20 -44 0 -2 -25 -1 -55 2 -97 10 -176 88 -214 211 -20 67 -37 209 -26 226 7 11 58 -15 140 -70z"/>
    </g>
  </svg>
</svg>`;

const ORYXBOT_ICON_URL = 'data:image/svg+xml;base64,' + btoa(ORYXBOT_ICON_SVG);

let map = null;
let markersLayer = null;
let edgesLayer = null;
let overlaysLayer = null;
let botsLayer = null;
let graphData = null;
let allClusters = [];      // marker objects
let clusterDataMap = {};   // id -> { cluster, loc, mapCenter, marker }
let locLookup = new Map(); // id -> location object (from albionLocations.json)
let activeOverlays = new Map(); // id -> imageOverlay
let loadingOverlays = new Set();
let bots = [];             // array of bot state objects
let botsAnimating = false;

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
        initBots(15);
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
    botsLayer = L.layerGroup().addTo(map);

    map.on('zoomend moveend', () => {
        updateMarkerSizes();
        updateOverlays();
    });
}

function processData(data, locations) {
    if (!data.clusters) return;

    if (locations) {
        locations.forEach(loc => {
            if (loc.id) locLookup.set(loc.id, loc);
            // Normalize bounds to {x, y} objects
            if (Array.isArray(loc.minimapBoundsMin)) {
                loc._boundsMin = { x: loc.minimapBoundsMin[0], y: loc.minimapBoundsMin[1] };
            } else if (loc.minimapBoundsMin) {
                loc._boundsMin = loc.minimapBoundsMin;
            }
            if (Array.isArray(loc.minimapBoundsMax)) {
                loc._boundsMax = { x: loc.minimapBoundsMax[0], y: loc.minimapBoundsMax[1] };
            } else if (loc.minimapBoundsMax) {
                loc._boundsMax = loc.minimapBoundsMax;
            }
            // Normalize exit positions to {x, y}
            [...(loc.exits || []), ...(loc.portalExits || []), ...(loc.portalEntrances || [])].forEach(exit => {
                if (Array.isArray(exit.position)) {
                    exit.position = { x: exit.position[0], y: exit.position[1] };
                }
            });
        });
        // Pre-resolve target display names and categories for exits
        locations.forEach(loc => {
            [...(loc.exits || []), ...(loc.portalExits || []), ...(loc.portalEntrances || [])].forEach(exit => {
                if (exit.targetLocationId) {
                    const target = locLookup.get(exit.targetLocationId);
                    if (target) {
                        exit._targetDisplayName = target.displayName;
                        exit._targetMapCategory = target.mapCategory;
                    }
                }
            });
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

        // Store mapCenter on loc so localToMapCoords can find it
        if (loc) loc.mapCenter = mapCenter;
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

    // Load overlays for visible clusters, sorted by distance to viewport center
    const center = map.getCenter();
    const visible = allClusters.filter(m => {
        if (!m._locData || !m._locData.imageFile) return false;
        if (activeOverlays.has(m._clusterData.id)) return false;
        if (loadingOverlays.has(m._clusterData.id)) return false;
        return bounds.contains(L.latLng(m._mapCenter.y, m._mapCenter.x));
    }).sort((a, b) => {
        const da = (a._mapCenter.y - center.lat) ** 2 + (a._mapCenter.x - center.lng) ** 2;
        const db = (b._mapCenter.y - center.lat) ** 2 + (b._mapCenter.x - center.lng) ** 2;
        return da - db;
    });

    // Load up to maxConcurrentLoads at a time
    const slots = CONFIG.maxConcurrentLoads - loadingOverlays.size;
    if (slots > 0) {
        visible.slice(0, slots).forEach(m => loadOverlay(m));
    }
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

        // Build layer group with image, labels, and exit markers
        const labelGroup = L.layerGroup();
        labelGroup.addLayer(imageOverlay);

        // Zone name label (top)
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

        // Tier label (bottom)
        const tierLabel = L.marker([mapCenter.y - h / 2, mapCenter.x], {
            icon: L.divIcon({
                className: `map-label small ${pvpClass}`,
                html: `<div>T${loc.tier || '?'}${loc.quality ? ' Q' + loc.quality : ''}</div>`,
                iconSize: null
            }),
            interactive: false
        });
        labelGroup.addLayer(tierLabel);

        // Exit markers (transitions to other zones/dungeons)
        addExitMarkers(labelGroup, loc, w, h);

        overlaysLayer.addLayer(labelGroup);
        activeOverlays.set(id, labelGroup);

        // Hide the circle marker
        marker.setStyle({ fillOpacity: 0, opacity: 0 });

        // Immediately kick off next queued loads
        updateOverlays();
    };

    img.onerror = () => {
        loadingOverlays.delete(id);
        // Still try to load remaining
        updateOverlays();
    };

    img.src = imgUrl;
}

function lerp(a, b, zoom, minZoom, maxZoom) {
    const t = (zoom - minZoom) / (maxZoom - minZoom);
    return a + (b - a) * Math.max(0, Math.min(1, t));
}

function addExitMarkers(layerGroup, loc, overlayW, overlayH) {
    const zoom = map.getZoom();
    const radius = lerp(
        CONFIG.coordinates.exits.radiusSmall,
        CONFIG.coordinates.exits.radiusLarge,
        zoom, CONFIG.minZoomForOverlays, CONFIG.map.options.maxZoom
    );

    const addMarker = (exit, fallbackName) => {
        if (!exit.position || !exit.targetLocationId) return;

        const coords = CoordTransform.localToMapCoords(exit.position, loc, overlayW, overlayH);
        if (!coords || isNaN(coords[0]) || isNaN(coords[1])) return;

        const targetCat = (exit._targetMapCategory || 'other').toLowerCase();
        const pvpKey = MAP_CATEGORY_PVP[targetCat] || 'blue';
        const fillColor = PVP_COLORS[pvpKey] || PVP_COLORS.other;
        const displayName = exit._targetDisplayName || fallbackName;

        const m = L.circleMarker(coords, {
            radius: radius,
            color: '#000',
            weight: 1,
            opacity: 1,
            fillColor: fillColor,
            fillOpacity: 0.9
        });

        m.bindTooltip(displayName, { direction: 'top', offset: [0, -5] });
        layerGroup.addLayer(m);
    };

    (loc.exits || []).forEach(e => addMarker(e, 'Exit'));
    (loc.portalExits || []).forEach(e => addMarker(e, 'Portal'));
    (loc.portalEntrances || []).forEach(e => addMarker(e, 'Portal'));
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

    ['filterWorld', 'filterCity', 'filterDungeon', 'filterBots'].forEach(id => {
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
    const showBots = document.getElementById('filterBots').checked;

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

    // Toggle bot layer
    if (showBots && !map.hasLayer(botsLayer)) {
        map.addLayer(botsLayer);
    } else if (!showBots && map.hasLayer(botsLayer)) {
        map.removeLayer(botsLayer);
    }
}

// ─── OryxBot Roaming System ───

function buildAdjacency() {
    const adj = {};
    if (!graphData || !graphData.edges) return adj;
    graphData.edges.forEach(e => {
        const from = e.fromClusterId;
        const to = e.toClusterId;
        if (clusterDataMap[from] && clusterDataMap[to]) {
            if (!adj[from]) adj[from] = [];
            if (!adj[to]) adj[to] = [];
            adj[from].push(to);
            adj[to].push(from);
        }
    });
    return adj;
}

function getRoyalNodes(adj) {
    // Royal Continent is the lower/southern portion of the map (more negative y)
    // Exclude dungeon/island/expedition interior types that aren't on the world map
    const excludeTypes = new Set(['DNG','ISL','EXP','TUNNEL_BLACK_LOW','TUNNEL_HIDEOUT',
        'TUNNEL_ROYAL','TUNNEL_LOW','TUNNEL_BLACK_MEDIUM','TUNNEL_HIDEOUT_DEEP',
        'TUNNEL_MEDIUM','TUNNEL_DEEP_RAID','TUNNEL_DEEP','TUNNEL_HIGH','TUNNEL_BLACK_HIGH',
        'ARENA_STANDARD','ARENA_CUSTOM','ARENA_CRYSTAL','ARENA_CRYSTAL_NONLETHAL','ARENA_CRYSTAL_20VS20',
        'HIDEOUT','HDO','HEL','HBS','COR','PGU','DBG','STT','TUTORIAL','SHOWROOMISLAND',
        'PLAYERISLAND','GUILDISLAND']);
    return Object.keys(adj).filter(id => {
        const d = clusterDataMap[id];
        if (!d || !d.mapCenter) return false;
        if (excludeTypes.has(d.cluster.type)) return false;
        return d.mapCenter.y < -130;
    });
}

function pickStartNodes(royalNodes, count) {
    // Uniformly distribute start positions across the Royal Continent
    if (royalNodes.length <= count) return royalNodes.slice();

    // Sort by position to spread them out, then pick evenly spaced
    const sorted = royalNodes.slice().sort((a, b) => {
        const da = clusterDataMap[a].mapCenter;
        const db = clusterDataMap[b].mapCenter;
        return (da.x + da.y * 1000) - (db.x + db.y * 1000);
    });

    const picks = [];
    const step = sorted.length / count;
    for (let i = 0; i < count; i++) {
        picks.push(sorted[Math.floor(i * step)]);
    }
    return picks;
}

function initBots(count) {
    const adj = buildAdjacency();
    const royalNodes = getRoyalNodes(adj);

    if (royalNodes.length < 2) return;

    const startNodes = pickStartNodes(royalNodes, count);
    const botIcon = L.icon({
        iconUrl: ORYXBOT_ICON_URL,
        iconSize: [32, 44],
        iconAnchor: [16, 44],
        className: 'oryxbot-marker'
    });

    startNodes.forEach(nodeId => {
        const pos = clusterDataMap[nodeId].mapCenter;
        const marker = L.marker([pos.y, pos.x], {
            icon: botIcon,
            interactive: true,
            zIndexOffset: 1000
        });
        marker.bindTooltip('OryxBot', { direction: 'top', offset: [0, -46] });
        botsLayer.addLayer(marker);

        bots.push({
            marker,
            currentNode: nodeId,
            prevNode: null,
            targetNode: null,
            fromPos: { lat: pos.y, lng: pos.x },
            toPos: null,
            startTime: 0,
            duration: 0,
            adj,
            royalNodes: new Set(royalNodes)
        });
    });

    // Start all bots with a random initial delay
    bots.forEach(bot => {
        setTimeout(() => pickNextAndAnimate(bot), Math.random() * 3000);
    });

    // Start animation loop
    if (!botsAnimating) {
        botsAnimating = true;
        requestAnimationFrame(animateBots);
    }
}

function pickNextNode(bot) {
    const neighbors = bot.adj[bot.currentNode];
    if (!neighbors || neighbors.length === 0) return null;

    // Filter to royal nodes, prefer not backtracking
    let candidates = neighbors.filter(n => bot.royalNodes.has(n) && n !== bot.prevNode);
    if (candidates.length === 0) {
        candidates = neighbors.filter(n => bot.royalNodes.has(n));
    }
    if (candidates.length === 0) {
        candidates = neighbors.filter(n => n !== bot.prevNode);
    }
    if (candidates.length === 0) {
        candidates = neighbors;
    }

    return candidates[Math.floor(Math.random() * candidates.length)];
}

function pickNextAndAnimate(bot) {
    const nextId = pickNextNode(bot);
    if (!nextId) return;

    const nextPos = clusterDataMap[nextId].mapCenter;
    bot.targetNode = nextId;
    bot.fromPos = bot.marker.getLatLng();
    bot.toPos = L.latLng(nextPos.y, nextPos.x);
    bot.startTime = performance.now();
    // 7-12 seconds per hop
    bot.duration = 7000 + Math.random() * 5000;
}

function animateBots(now) {
    bots.forEach(bot => {
        if (!bot.toPos || !bot.startTime) return;

        let t = (now - bot.startTime) / bot.duration;
        if (t >= 1) {
            t = 1;
            bot.marker.setLatLng(bot.toPos);
            bot.prevNode = bot.currentNode;
            bot.currentNode = bot.targetNode;
            bot.targetNode = null;
            bot.toPos = null;
            // Pick next hop
            setTimeout(() => pickNextAndAnimate(bot), 500 + Math.random() * 1500);
            return;
        }

        // Ease in-out for smooth movement
        const ease = t < 0.5 ? 2 * t * t : 1 - Math.pow(-2 * t + 2, 2) / 2;
        const lat = bot.fromPos.lat + (bot.toPos.lat - bot.fromPos.lat) * ease;
        const lng = bot.fromPos.lng + (bot.toPos.lng - bot.fromPos.lng) * ease;
        bot.marker.setLatLng([lat, lng]);
    });

    requestAnimationFrame(animateBots);
}
