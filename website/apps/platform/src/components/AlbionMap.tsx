import { useEffect, useRef, useCallback } from 'react'
import L from 'leaflet'
import 'leaflet/dist/leaflet.css'

// ── Types ──

interface AlbionMapProps {
    /** Leaflet zoom level (0–7). Default: 3 */
    zoom?: number
    /** Map center as [lat, lng] in map coordinates. Default: Royal Continent center */
    center?: [number, number]
    /** Allow user interaction (drag, zoom, click). Default: true */
    interactive?: boolean
    /** Show minimap overlays when zoomed in enough. Default: true */
    showOverlays?: boolean
    /** Show cluster circle markers. Default: true */
    showMarkers?: boolean
    /** Show edge polylines. Default: true */
    showEdges?: boolean
    /** Number of animated OryxBot icons. 0 to disable. Default: 0 */
    botCount?: number
    /** Extra CSS class for the map container */
    className?: string
    /** Inline styles for the map container */
    style?: React.CSSProperties
    /** Base path to map data files. Default: '/map/data' */
    dataPath?: string
}

// ── Constants ──

const PVP_COLORS: Record<string, string> = {
    blue: '#3a86ff', yellow: '#ffd60a', red: '#e63946',
    black: '#2d2d2d', white: '#ffffff', green: '#2a9d8f', other: '#888888'
}

const MAP_CATEGORY_PVP: Record<string, string> = {
    arena: 'blue', corrupted: 'blue', dungeon: 'red', island: 'blue',
    expedition: 'blue', hideout: 'blue', portalcity: 'white', hellden: 'blue',
    startingcity: 'white', rest: 'white', city: 'white', startarea: 'blue',
    debug_black: 'blue', openworld: 'blue', passage: 'green', roads: 'blue', other: 'blue'
}

const EXCLUDE_TYPES = new Set([
    'DNG', 'ISL', 'EXP', 'TUNNEL_BLACK_LOW', 'TUNNEL_HIDEOUT',
    'TUNNEL_ROYAL', 'TUNNEL_LOW', 'TUNNEL_BLACK_MEDIUM', 'TUNNEL_HIDEOUT_DEEP',
    'TUNNEL_MEDIUM', 'TUNNEL_DEEP_RAID', 'TUNNEL_DEEP', 'TUNNEL_HIGH', 'TUNNEL_BLACK_HIGH',
    'ARENA_STANDARD', 'ARENA_CUSTOM', 'ARENA_CRYSTAL', 'ARENA_CRYSTAL_NONLETHAL', 'ARENA_CRYSTAL_20VS20',
    'HIDEOUT', 'HDO', 'HEL', 'HBS', 'COR', 'PGU', 'DBG', 'STT', 'TUTORIAL', 'SHOWROOMISLAND',
    'PLAYERISLAND', 'GUILDISLAND'
])

const COORD_CONFIG = {
    xOffset: 128, yOffset: -128,
    scaleFactor: 200,
    gameScale: { sourceRange: 800, targetRange: 256 },
    rotation: { angle: -45, scaleAfterRotation: 0.7 },
    exits: { radiusSmall: 5, radiusLarge: 10 }
}

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
</svg>`

// ── Coordinate Transform ──

const CoordTransform = {
    gameToMap(val: number) {
        return val / COORD_CONFIG.gameScale.sourceRange * COORD_CONFIG.gameScale.targetRange
    },
    applyOffsets([x, y]: [number, number]): [number, number] {
        return [x + COORD_CONFIG.xOffset, y + COORD_CONFIG.yOffset]
    },
    worldToMapCenter(worldmapposition: number[] | { x: number; y: number }) {
        const wmp = Array.isArray(worldmapposition)
            ? worldmapposition
            : [worldmapposition.x, worldmapposition.y]
        const mx = this.gameToMap(wmp[0])
        const my = this.gameToMap(wmp[1])
        const [x, y] = this.applyOffsets([mx, my])
        return { x, y }
    },
    rotateAndScale([x, y]: [number, number]): [number, number] {
        const angle = COORD_CONFIG.rotation.angle * Math.PI / 180
        const cos = Math.cos(angle)
        const sin = Math.sin(angle)
        const s = COORD_CONFIG.rotation.scaleAfterRotation
        return [(x * cos - y * sin) * s, (x * sin + y * cos) * s]
    },
    localToMapCoords(
        pos: { x: number; y: number },
        loc: { mapCenter?: { x: number; y: number }; _boundsMin?: { x: number; y: number }; _boundsMax?: { x: number; y: number } },
        overlayW: number, overlayH: number
    ) {
        if (!loc.mapCenter) return null
        const bMin = loc._boundsMin
        const bMax = loc._boundsMax
        if (!bMin || !bMax) return null

        const cx = (bMin.x + bMax.x) / 2
        const cy = (bMin.y + bMax.y) / 2

        let dx = pos.x - cx
        let dy = pos.y - cy
        ;[dx, dy] = this.rotateAndScale([dx, dy])

        const normX = (dx + cx - bMin.x) / (bMax.x - bMin.x)
        const normY = (dy + cy - bMin.y) / (bMax.y - bMin.y)

        const lat = loc.mapCenter.y - overlayH / 2 + normY * overlayH
        const lng = loc.mapCenter.x - overlayW / 2 + normX * overlayW
        return [lat, lng] as [number, number]
    }
}

// ── Helper Types ──

interface ClusterData {
    id: string
    displayName?: string
    type?: string
    tier?: string
    biome?: string
    zone?: string
    worldX?: number
    worldY?: number
}

interface LocationData {
    id: string
    displayName?: string
    worldmapposition?: number[] | { x: number; y: number }
    pvpCategory?: string
    mapCategory?: string
    tier?: string
    quality?: string
    type?: string
    biome?: string
    imageFile?: string
    minimapBoundsMin?: number[] | { x: number; y: number }
    minimapBoundsMax?: number[] | { x: number; y: number }
    exits?: ExitData[]
    portalExits?: ExitData[]
    portalEntrances?: ExitData[]
    // runtime
    _boundsMin?: { x: number; y: number }
    _boundsMax?: { x: number; y: number }
    mapCenter?: { x: number; y: number }
}

interface ExitData {
    position?: number[] | { x: number; y: number }
    targetLocationId?: string
    _targetDisplayName?: string
    _targetMapCategory?: string
}

interface GraphData {
    clusters: Record<string, ClusterData>
    edges: { fromClusterId: string; toClusterId: string }[]
}

interface BotState {
    marker: L.Marker
    currentNode: string
    prevNode: string | null
    targetNode: string | null
    fromPos: { lat: number; lng: number }
    toPos: L.LatLng | null
    startTime: number
    duration: number
}

// ── Component ──

export function AlbionMap({
    zoom = 3,
    center = [-195, 130],
    interactive = true,
    showOverlays = true,
    showMarkers = true,
    showEdges = true,
    botCount = 0,
    className = '',
    style,
    dataPath = '/map/data'
}: AlbionMapProps) {
    const containerRef = useRef<HTMLDivElement>(null)
    const mapRef = useRef<L.Map | null>(null)
    const botsRef = useRef<BotState[]>([])
    const animFrameRef = useRef<number>(0)
    const clusterDataMapRef = useRef<Record<string, {
        cluster: ClusterData; loc: LocationData | null; mapCenter: { x: number; y: number }; marker: L.CircleMarker
    }>>({})
    const activeOverlaysRef = useRef<Map<string, L.LayerGroup>>(new Map())
    const loadingOverlaysRef = useRef<Set<string>>(new Set())
    const adjRef = useRef<Record<string, string[]>>({})
    const royalNodesRef = useRef<Set<string>>(new Set())
    const overlaysLayerRef = useRef<L.LayerGroup | null>(null)

    const minZoomForOverlays = 4
    const maxConcurrentLoads = 20

    // ── Overlay loading ──

    const loadOverlay = useCallback((
        marker: L.CircleMarker & { _clusterData: ClusterData; _locData: LocationData; _mapCenter: { x: number; y: number } },
        map: L.Map
    ) => {
        const loc = marker._locData
        const id = marker._clusterData.id
        const mapCenter = marker._mapCenter

        if (!loc || !loc.imageFile) return

        loadingOverlaysRef.current.add(id)

        const webpName = loc.imageFile.replace('.png', '.webp')
        const imgUrl = `${dataPath}/tiles/images/${webpName}`

        const img = new Image()
        img.onload = () => {
            loadingOverlaysRef.current.delete(id)
            if (map.getZoom() < minZoomForOverlays) return

            const w = img.naturalWidth / COORD_CONFIG.scaleFactor
            const h = img.naturalHeight / COORD_CONFIG.scaleFactor

            const overlayBounds: L.LatLngBoundsExpression = [
                [mapCenter.y - h / 2, mapCenter.x - w / 2],
                [mapCenter.y + h / 2, mapCenter.x + w / 2]
            ]

            const imageOverlay = L.imageOverlay(imgUrl, overlayBounds, { interactive: true })

            const labelGroup = L.layerGroup()
            labelGroup.addLayer(imageOverlay)

            const pvpClass = `pvp-${loc.pvpCategory || 'other'}`
            labelGroup.addLayer(L.marker([mapCenter.y + h / 2, mapCenter.x], {
                icon: L.divIcon({
                    className: `map-label ${pvpClass}`,
                    html: `<div>${loc.displayName || id}</div>`,
                    iconSize: undefined
                }),
                interactive: false
            }))

            labelGroup.addLayer(L.marker([mapCenter.y - h / 2, mapCenter.x], {
                icon: L.divIcon({
                    className: `map-label small ${pvpClass}`,
                    html: `<div>T${loc.tier || '?'}${loc.quality ? ' Q' + loc.quality : ''}</div>`,
                    iconSize: undefined
                }),
                interactive: false
            }))

            // Exit markers
            const exitRadius = 5 + (map.getZoom() - minZoomForOverlays) / (7 - minZoomForOverlays) * 5
            const addExitMarker = (exit: ExitData, fallbackName: string) => {
                if (!exit.position || !exit.targetLocationId) return
                const pos = typeof (exit.position as { x: number }).x === 'number'
                    ? exit.position as { x: number; y: number }
                    : { x: (exit.position as number[])[0], y: (exit.position as number[])[1] }
                const coords = CoordTransform.localToMapCoords(pos, loc, w, h)
                if (!coords || isNaN(coords[0]) || isNaN(coords[1])) return
                const targetCat = (exit._targetMapCategory || 'other').toLowerCase()
                const pvpKey = MAP_CATEGORY_PVP[targetCat] || 'blue'
                const fillColor = PVP_COLORS[pvpKey] || PVP_COLORS.other
                const m = L.circleMarker(coords, {
                    radius: exitRadius, color: '#000', weight: 1, opacity: 1,
                    fillColor, fillOpacity: 0.9
                })
                m.bindTooltip(exit._targetDisplayName || fallbackName, { direction: 'top', offset: [0, -5] })
                labelGroup.addLayer(m)
            }
            ;(loc.exits || []).forEach(e => addExitMarker(e, 'Exit'))
            ;(loc.portalExits || []).forEach(e => addExitMarker(e, 'Portal'))
            ;(loc.portalEntrances || []).forEach(e => addExitMarker(e, 'Portal'))

            overlaysLayerRef.current?.addLayer(labelGroup)
            activeOverlaysRef.current.set(id, labelGroup)

            marker.setStyle({ fillOpacity: 0, opacity: 0 })
            updateOverlays(map)
        }
        img.onerror = () => {
            loadingOverlaysRef.current.delete(id)
            updateOverlays(map)
        }
        img.src = imgUrl
    }, [dataPath])

    const updateOverlays = useCallback((map: L.Map) => {
        const zoomLevel = map.getZoom()
        const clusterDataMap = clusterDataMapRef.current
        const activeOverlays = activeOverlaysRef.current
        const loadingOverlays = loadingOverlaysRef.current
        const overlaysLayer = overlaysLayerRef.current

        if (zoomLevel < minZoomForOverlays) {
            overlaysLayer?.clearLayers()
            activeOverlays.clear()
            return
        }

        const bounds = map.getBounds().pad(0.3)

        for (const [id, overlay] of activeOverlays) {
            const data = clusterDataMap[id]
            if (!data || !bounds.contains(L.latLng(data.mapCenter.y, data.mapCenter.x))) {
                overlaysLayer?.removeLayer(overlay)
                activeOverlays.delete(id)
                if (data?.marker) data.marker.setStyle({ fillOpacity: 0.9, opacity: 1 })
            }
        }

        const center = map.getCenter()
        const allMarkers = Object.values(clusterDataMap)
        const visible = allMarkers.filter(d => {
            if (!d.loc?.imageFile) return false
            if (activeOverlays.has(d.cluster.id)) return false
            if (loadingOverlays.has(d.cluster.id)) return false
            return bounds.contains(L.latLng(d.mapCenter.y, d.mapCenter.x))
        }).sort((a, b) => {
            const da = (a.mapCenter.y - center.lat) ** 2 + (a.mapCenter.x - center.lng) ** 2
            const db = (b.mapCenter.y - center.lat) ** 2 + (b.mapCenter.x - center.lng) ** 2
            return da - db
        })

        const slots = maxConcurrentLoads - loadingOverlays.size
        if (slots > 0) {
            visible.slice(0, slots).forEach(d => {
                const m = d.marker as L.CircleMarker & { _clusterData: ClusterData; _locData: LocationData; _mapCenter: { x: number; y: number } }
                loadOverlay(m, map)
            })
        }
    }, [loadOverlay])

    // ── Main setup effect ──

    useEffect(() => {
        if (!containerRef.current) return

        const map = L.map(containerRef.current, {
            crs: L.CRS.Simple,
            minZoom: 0,
            maxZoom: 7,
            zoomAnimationThreshold: 0,
            attributionControl: false,
            zoomAnimation: true,
            fadeAnimation: true,
            markerZoomAnimation: true,
            // Interaction controls
            dragging: interactive,
            touchZoom: interactive,
            doubleClickZoom: interactive,
            scrollWheelZoom: interactive,
            boxZoom: interactive,
            keyboard: interactive,
            zoomControl: interactive
        }).setView(center, zoom)

        mapRef.current = map

        L.tileLayer(`${dataPath}/tiles/maps/{z}/map_{x}_{y}.webp`, {
            tileSize: 256,
            noWrap: true,
            maxNativeZoom: 6,
            minZoom: 0,
            zoomOffset: 0
        }).addTo(map)

        const edgesLayer = L.layerGroup().addTo(map)
        const overlaysLayer = L.layerGroup().addTo(map)
        overlaysLayerRef.current = overlaysLayer
        const markersLayer = L.layerGroup().addTo(map)
        const botsLayer = L.layerGroup().addTo(map)

        // Load data
        Promise.all([
            fetch(`${dataPath}/world-graph.json`).then(r => r.json()) as Promise<GraphData>,
            fetch(`${dataPath}/albionLocations.json`).then(r => r.json()).catch(() => null) as Promise<LocationData[] | null>
        ]).then(([graphData, locations]) => {
            const locLookup = new Map<string, LocationData>()
            const clusterDataMap: typeof clusterDataMapRef.current = {}

            // Process locations
            if (locations) {
                locations.forEach(loc => {
                    if (loc.id) locLookup.set(loc.id, loc)
                    if (Array.isArray(loc.minimapBoundsMin))
                        loc._boundsMin = { x: loc.minimapBoundsMin[0], y: loc.minimapBoundsMin[1] }
                    else if (loc.minimapBoundsMin)
                        loc._boundsMin = loc.minimapBoundsMin as { x: number; y: number }
                    if (Array.isArray(loc.minimapBoundsMax))
                        loc._boundsMax = { x: loc.minimapBoundsMax[0], y: loc.minimapBoundsMax[1] }
                    else if (loc.minimapBoundsMax)
                        loc._boundsMax = loc.minimapBoundsMax as { x: number; y: number }

                    const allExits = [...(loc.exits || []), ...(loc.portalExits || []), ...(loc.portalEntrances || [])]
                    allExits.forEach(exit => {
                        if (Array.isArray(exit.position))
                            exit.position = { x: exit.position[0], y: exit.position[1] }
                    })
                })
                locations.forEach(loc => {
                    const allExits = [...(loc.exits || []), ...(loc.portalExits || []), ...(loc.portalEntrances || [])]
                    allExits.forEach(exit => {
                        if (exit.targetLocationId) {
                            const target = locLookup.get(exit.targetLocationId)
                            if (target) {
                                exit._targetDisplayName = target.displayName
                                exit._targetMapCategory = target.mapCategory
                            }
                        }
                    })
                })
            }

            // Process clusters
            Object.values(graphData.clusters).forEach(c => {
                let mapCenter = null
                const loc = locLookup.get(c.id) || null

                if (loc?.worldmapposition) {
                    const wmp = Array.isArray(loc.worldmapposition)
                        ? loc.worldmapposition
                        : [loc.worldmapposition.x, loc.worldmapposition.y]
                    mapCenter = CoordTransform.worldToMapCenter(wmp)
                }

                if (!mapCenter && c.worldX !== undefined && c.worldY !== undefined) {
                    mapCenter = CoordTransform.worldToMapCenter([c.worldX, c.worldY])
                }

                if (!mapCenter) return

                const pvp = (loc?.pvpCategory) || ''
                const color = PVP_COLORS[pvp] || PVP_COLORS.other
                const displayName = c.displayName || loc?.displayName || c.id
                const tierStr = c.tier || loc?.tier || '?'

                const marker = L.circleMarker([mapCenter.y, mapCenter.x], {
                    radius: 4, color: '#000', weight: 1, opacity: 1,
                    fillColor: color, fillOpacity: 0.9
                }) as L.CircleMarker & { _clusterData: ClusterData; _locData: LocationData; _mapCenter: { x: number; y: number }; _displayName: string }

                marker.bindTooltip(
                    `<b>${displayName}</b><br>T${tierStr} | ${pvp || c.zone || '?'}`,
                    { direction: 'top', offset: [0, -5] }
                )

                marker._clusterData = c
                marker._locData = loc!
                marker._mapCenter = mapCenter
                marker._displayName = displayName

                if (showMarkers) markersLayer.addLayer(marker)
                if (loc) loc.mapCenter = mapCenter
                clusterDataMap[c.id] = { cluster: c, loc, mapCenter, marker }
            })

            clusterDataMapRef.current = clusterDataMap

            // Edges
            if (showEdges && graphData.edges) {
                graphData.edges.forEach(e => {
                    const from = clusterDataMap[e.fromClusterId]
                    const to = clusterDataMap[e.toClusterId]
                    if (from && to) {
                        L.polyline(
                            [[from.mapCenter.y, from.mapCenter.x], [to.mapCenter.y, to.mapCenter.x]],
                            { color: '#555', weight: 1, opacity: 0.4 }
                        ).addTo(edgesLayer)
                    }
                })
            }

            // Build adjacency for bots
            const adj: Record<string, string[]> = {}
            graphData.edges.forEach(e => {
                const f = e.fromClusterId, t = e.toClusterId
                if (clusterDataMap[f] && clusterDataMap[t]) {
                    if (!adj[f]) adj[f] = []
                    if (!adj[t]) adj[t] = []
                    adj[f].push(t)
                    adj[t].push(f)
                }
            })
            adjRef.current = adj

            const royalNodes = Object.keys(adj).filter(id => {
                const d = clusterDataMap[id]
                if (!d?.mapCenter) return false
                if (EXCLUDE_TYPES.has(d.cluster.type || '')) return false
                return d.mapCenter.y < -130
            })
            royalNodesRef.current = new Set(royalNodes)

            // Overlays
            if (showOverlays) {
                map.on('zoomend moveend', () => updateOverlays(map))
                updateOverlays(map)
            }

            // Bots
            if (botCount > 0 && royalNodes.length >= 2) {
                const sorted = royalNodes.slice().sort((a, b) => {
                    const da = clusterDataMap[a].mapCenter
                    const db = clusterDataMap[b].mapCenter
                    return (da.x + da.y * 1000) - (db.x + db.y * 1000)
                })
                const step = sorted.length / botCount
                const startNodes = Array.from({ length: botCount }, (_, i) =>
                    sorted[Math.floor(i * step)]
                )

                const botIcon = L.icon({
                    iconUrl: 'data:image/svg+xml;base64,' + btoa(ORYXBOT_ICON_SVG),
                    iconSize: [32, 44],
                    iconAnchor: [16, 44],
                    className: 'oryxbot-marker'
                })

                const bots: BotState[] = startNodes.map(nodeId => {
                    const pos = clusterDataMap[nodeId].mapCenter
                    const marker = L.marker([pos.y, pos.x], {
                        icon: botIcon, interactive: false, zIndexOffset: 1000
                    })
                    botsLayer.addLayer(marker)
                    return {
                        marker, currentNode: nodeId, prevNode: null, targetNode: null,
                        fromPos: { lat: pos.y, lng: pos.x }, toPos: null,
                        startTime: 0, duration: 0
                    }
                })
                botsRef.current = bots

                const pickNext = (bot: BotState) => {
                    const neighbors = adj[bot.currentNode]
                    if (!neighbors?.length) return
                    let candidates = neighbors.filter(n => royalNodesRef.current.has(n) && n !== bot.prevNode)
                    if (!candidates.length) candidates = neighbors.filter(n => royalNodesRef.current.has(n))
                    if (!candidates.length) candidates = neighbors.filter(n => n !== bot.prevNode)
                    if (!candidates.length) candidates = neighbors
                    const nextId = candidates[Math.floor(Math.random() * candidates.length)]
                    const nextPos = clusterDataMap[nextId].mapCenter
                    bot.targetNode = nextId
                    bot.fromPos = bot.marker.getLatLng()
                    bot.toPos = L.latLng(nextPos.y, nextPos.x)
                    bot.startTime = performance.now()
                    bot.duration = 7000 + Math.random() * 5000
                }

                bots.forEach(bot => {
                    setTimeout(() => pickNext(bot), Math.random() * 3000)
                })

                const animate = (now: number) => {
                    bots.forEach(bot => {
                        if (!bot.toPos || !bot.startTime) return
                        let t = (now - bot.startTime) / bot.duration
                        if (t >= 1) {
                            bot.marker.setLatLng(bot.toPos)
                            bot.prevNode = bot.currentNode
                            bot.currentNode = bot.targetNode!
                            bot.targetNode = null
                            bot.toPos = null
                            setTimeout(() => pickNext(bot), 500 + Math.random() * 1500)
                            return
                        }
                        const ease = t < 0.5 ? 2 * t * t : 1 - Math.pow(-2 * t + 2, 2) / 2
                        const lat = bot.fromPos.lat + (bot.toPos.lat - bot.fromPos.lat) * ease
                        const lng = bot.fromPos.lng + (bot.toPos.lng - bot.fromPos.lng) * ease
                        bot.marker.setLatLng([lat, lng])
                    })
                    animFrameRef.current = requestAnimationFrame(animate)
                }
                animFrameRef.current = requestAnimationFrame(animate)
            }
        })

        return () => {
            cancelAnimationFrame(animFrameRef.current)
            map.remove()
            mapRef.current = null
        }
    }, []) // eslint-disable-line react-hooks/exhaustive-deps

    return (
        <div
            ref={containerRef}
            className={className}
            style={{
                width: '100%',
                height: '100%',
                background: '#30312a',
                ...style
            }}
        />
    )
}
