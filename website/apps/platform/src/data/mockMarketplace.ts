import type { MarketplaceEntry } from '../components/MarketplaceGrid'

export const MARKETPLACE_ENTRIES: MarketplaceEntry[] = [
    {
        name: 'FortFisher Pro',
        author: '@anglerdev',
        price: '€8/mo',
        description: 'Automated fishing at Fort Sterling docks. Auto-sells trash fish, banks rare catches.',
        rating: 4,
        category: 'Fishing Scripts',
    },
    {
        name: 'Stonemouth Miner',
        author: '@oreminer42',
        price: '€12/mo',
        description: 'T6 ore gathering in Stonemouth Rills. Fills inventory → deposits at Fort Sterling → repeats.',
        rating: 5,
        category: 'Gathering Bots',
    },
    {
        name: 'Caerleon Courier',
        author: '@fastroutes',
        price: '€5/mo',
        description: 'Optimized trade route runner between Caerleon and all royal cities.',
        rating: 4,
        category: 'Navigation Scripts',
    },
    {
        name: 'HideHunter',
        author: '@skinnerbot',
        price: '€10/mo',
        description: 'T5-T7 hide gathering across multiple Steppe zones with PvP flee logic.',
        rating: 4,
        category: 'Gathering Bots',
    },
    {
        name: 'WoodCutter Elite',
        author: '@lumberai',
        price: '€7/mo',
        description: 'Logs T5-T8 wood in rotating forest zones. Smart tree selection.',
        rating: 3,
        category: 'Gathering Bots',
    },
    {
        name: 'GatherAll',
        author: '@multipurpose',
        price: '€15/mo',
        description: 'All-resource gatherer. Configurable target resource, tier, and zone.',
        rating: 5,
        category: 'Utility Scripts',
    },
]

export const MARKETPLACE_CATEGORIES = [
    'All',
    'Navigation Scripts',
    'Gathering Bots',
    'Fishing Scripts',
    'Utility Scripts',
]
