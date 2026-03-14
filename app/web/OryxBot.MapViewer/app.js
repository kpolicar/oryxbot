let network = null;
let nodesData = new vis.DataSet([]);
let edgesData = new vis.DataSet([]);
let allNodes = [];
let allEdges = [];
let graphData = null;

document.addEventListener('DOMContentLoaded', () => {
    initApp();
});

async function initApp() {
    try {
        const response = await fetch('data/world-graph.json?v=3');
        if (!response.ok) {
            throw new Error(`Failed to load data: ${response.statusText}`);
        }
        graphData = await response.json();
        
        document.getElementById('loading').style.display = 'none';
        processData(graphData);
        drawNetwork();
        setupEventListeners();
    } catch (error) {
        document.getElementById('loading').innerText = 'Error: Data not found. Ensure --output is mapped to data/world-graph.json';
        console.error(error);
    }
}

function processData(data) {
    if (!data.clusters) return;

    Object.values(data.clusters).forEach(c => {
        let color = '#4ade80'; // WORLD
        if (c.type === 'CITY') color = '#fbbf24';
        else if (c.type === 'DUNGEON') color = '#9ca3af';

        // Try to load a minimap tile image from the CDN based on the cluster's internal name.
        // For example, internalName is something like "4210_WRL_MN_AUTO_T3_HER_ROY"
        let imagePath = null;
        if (c.internalName) {
            imagePath = `https://cdn.albionfreemarket.com/AlbionWorld/map/images/${c.internalName}.webp`;
        } else {
            // Fallback to our local biome textures if internalName isn't present
            let biomeImg = 'map_albion.png';
            if (c.biome === 'ST') biomeImg = 'map_albion_steppe.png';
            else if (c.biome === 'FR') biomeImg = 'map_albion_forest.png';
            else if (c.biome === 'MN') biomeImg = 'map_albion_mountain.png';
            else if (c.biome === 'SW') biomeImg = 'map_albion_swamp.png';
            else if (c.biome === 'HL') biomeImg = 'map_albion_highlands.png';
            imagePath = `data/tiles/${biomeImg}`;
        }
        
        let node = {
            id: c.id,
            label: c.displayName || c.id,
            title: `ID: ${c.id}<br>Type: ${c.type}<br>Tier: ${c.tier || 'N/A'}<br>Biome: ${c.biome || 'N/A'}<br>File: ${c.internalName || 'N/A'}<br>Coords: ${c.worldX}, ${c.worldY}`,
            group: c.type,
            color: { background: color, border: '#222' },
            font: { color: '#fff' },
            shape: 'image', // try loading image
            image: imagePath,
            brokenImage: `data:image/svg+xml;utf8,<svg xmlns="http://www.w3.org/2000/svg" width="60" height="60"><rect width="60" height="60" fill="${encodeURIComponent(color)}" rx="8" stroke="%23222" stroke-width="2"/></svg>`,
            size: 35 // larger size for images to create a tiled effect
        };

        if (c.worldX !== undefined && c.worldY !== undefined) {
            node.x = c.worldX * 15;
            node.y = -c.worldY * 15; 
            allNodes.push(node);
        }
    });

    if (data.edges) {
        data.edges.forEach(e => {
            allEdges.push({
                from: e.fromClusterId,
                to: e.toClusterId,
                color: { color: '#444', highlight: '#fbbf24' }
            });
        });
    }

    nodesData.add(allNodes);
    edgesData.add(allEdges);
}

function drawNetwork() {
    const container = document.getElementById('network-container');
    const data = {
        nodes: nodesData,
        edges: edgesData
    };
    const options = {
        nodes: {
            shape: 'dot',
            size: 30, // Default for non-image nodes
            borderWidth: 2,
            shapeProperties: {
                useBorderWithImage: true
            }
        },
        edges: {
            width: 1,
            smooth: {
                type: 'continuous'
            }
        },
        physics: false,
        interaction: {
            hover: true,
            tooltipDelay: 200
        }
    };

    network = new vis.Network(container, data, options);

    // Load background image
    const bgImage = new Image();
    bgImage.src = 'data/tiles/worldmap_upscaled.png';

    // Hook into the canvas drawing to paint the map underneath nodes
    network.on('beforeDrawing', function(ctx) {
        if (!bgImage.complete) return;

        // The image is 512x1024.
        // Our nodes are scaled by a factor of 15 from their natural worldX and worldY coordinates.
        // The game origin (0,0) represents the exact center of this image (256, 512 in pixel space).
        
        const scale = 15;
        const mapWidth = 512 * scale; 
        const mapHeight = 1024 * scale; 
        
        // Center the image around the origin 0,0
        const offsetX = 0;
        const offsetY = 0;

        ctx.drawImage(
            bgImage,
            offsetX - (mapWidth / 2),
            offsetY - (mapHeight / 2),
            mapWidth,
            mapHeight
        );
    });

    network.on('click', function(params) {
        if (params.nodes.length > 0) {
            const nodeId = params.nodes[0];
            showClusterInfo(nodeId);
        } else {
            document.getElementById('infoPanel').classList.add('hidden');
        }
    });
}

function showClusterInfo(clusterId) {
    if (!graphData || !graphData.clusters || !graphData.clusters[clusterId]) return;
    
    const cluster = graphData.clusters[clusterId];
    const panel = document.getElementById('infoPanel');
    const details = document.getElementById('clusterDetails');
    
    panel.classList.remove('hidden');
    details.innerHTML = `
        <p><strong>ID:</strong> ${cluster.id}</p>
        <p><strong>Name:</strong> ${cluster.displayName || 'Unknown'}</p>
        <p><strong>Type:</strong> ${cluster.type}</p>
        <p><strong>Tier:</strong> ${cluster.tier || 'N/A'}</p>
        <p><strong>Biome:</strong> ${cluster.biome || 'N/A'}</p>
        <p><strong>Zone:</strong> ${cluster.zone || 'N/A'}</p>
    `;
}

function setupEventListeners() {
    // Search
    document.getElementById('searchBtn').addEventListener('click', () => {
        const term = document.getElementById('searchInput').value.toLowerCase();
        if (!term) return;

        const node = allNodes.find(n => n.id === term || n.label.toLowerCase().includes(term));
        if (node) {
            network.focus(node.id, {
                scale: 1.5,
                animation: { duration: 1000, easingFunction: 'easeInOutQuad' }
            });
            network.selectNodes([node.id]);
            showClusterInfo(node.id);
        }
    });

    // Filters
    const filterIds = ['filterWorld', 'filterCity', 'filterDungeon'];
    filterIds.forEach(id => {
        document.getElementById(id).addEventListener('change', updateFilters);
    });
}

function updateFilters() {
    const showWorld = document.getElementById('filterWorld').checked;
    const showCity = document.getElementById('filterCity').checked;
    const showDungeon = document.getElementById('filterDungeon').checked;

    const filteredNodes = allNodes.filter(n => {
        if (n.group === 'WORLD' && !showWorld) return false;
        if (n.group === 'CITY' && !showCity) return false;
        if (n.group === 'DUNGEON' && !showDungeon) return false;
        return true;
    });

    const filteredIds = new Set(filteredNodes.map(n => n.id));
    
    const filteredEdges = allEdges.filter(e => 
        filteredIds.has(e.from) && filteredIds.has(e.to)
    );

    nodesData.clear();
    edgesData.clear();
    
    nodesData.add(filteredNodes);
    edgesData.add(filteredEdges);
}
