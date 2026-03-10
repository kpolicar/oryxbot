import { motion } from 'framer-motion'
import { Server, Shield, Monitor, Cloud } from 'lucide-react'

export function ArchitectureSection() {
    return (
        <section className="py-28 px-6 relative overflow-hidden">
            <div className="max-w-6xl mx-auto grid grid-cols-1 lg:grid-cols-2 gap-16 items-center">

                {/* Text Column (Left) */}
                <motion.div
                    initial={{ opacity: 0, x: -30 }}
                    whileInView={{ opacity: 1, x: 0 }}
                    viewport={{ once: true }}
                    transition={{ duration: 0.6 }}
                    className="text-left"
                >
                    <span className="text-[11px] font-medium tracking-[0.15em] uppercase text-accent-green block mb-3">
                        Kernel-level Security
                    </span>
                    <h2 className="text-3xl lg:text-5xl font-bold text-text-primary mb-6 leading-tight">
                        Architected to be <br /> <span className="text-gradient-gold">undetectable.</span>
                    </h2>
                    <p className="text-text-secondary text-lg leading-relaxed mb-6">
                        OryxBot operates entirely outside the game client. By leveraging a separate machine or a deeply isolated VM, it completely bypasses conventional anti-cheat detection methods like memory scanning.
                    </p>

                    <ul className="space-y-4">
                        <li className="flex items-start gap-4">
                            <div className="p-2 rounded-lg bg-bg-surface text-text-primary mt-1 border border-border-subtle">
                                <Monitor size={16} />
                            </div>
                            <div>
                                <h4 className="font-semibold text-text-primary text-sm mb-1">Network-Only Interaction</h4>
                                <p className="text-xs text-text-muted leading-relaxed">
                                    The bot interacts entirely via packet analysis and hardware-level simulated inputs. It never injects into the game.
                                </p>
                            </div>
                        </li>
                        <li className="flex items-start gap-4">
                            <div className="p-2 rounded-lg bg-bg-surface text-text-primary mt-1 border border-border-subtle">
                                <Shield size={16} />
                            </div>
                            <div>
                                <h4 className="font-semibold text-text-primary text-sm mb-1">Zero Memory Footprint</h4>
                                <p className="text-xs text-text-muted leading-relaxed">
                                    Anti-cheat software relies on scanning local memory for tampering. Our separate-machine architecture means there is nothing to detect.
                                </p>
                            </div>
                        </li>
                    </ul>
                </motion.div>

                {/* Architecture Diagram Column (Right) */}
                <motion.div
                    initial={{ opacity: 0, x: 30 }}
                    whileInView={{ opacity: 1, x: 0 }}
                    viewport={{ once: true }}
                    transition={{ duration: 0.6, delay: 0.2 }}
                    className="relative rounded-2xl p-[1px] overflow-hidden group shadow-2xl"
                >
                    {/* Spinning Gradient Border */}
                    <div
                        className="absolute -inset-[100%] z-0 opacity-50 group-hover:opacity-100 transition-opacity duration-1000"
                        style={{
                            background: 'conic-gradient(from 0deg, transparent 0%, transparent 30%, rgba(211,200,168,0.5) 50%, transparent 70%, transparent 100%)',
                            animation: 'spin-gradient 8s linear infinite'
                        }}
                    />

                    {/* Inner Card Background */}
                    <div className="absolute inset-[1px] rounded-2xl bg-bg-card z-0" />

                    {/* Content Container */}
                    <div className="relative z-10 w-full p-4 sm:p-8 flex flex-col items-center justify-center">
                        <div className="relative w-full shadow-inner shadow-black/20 rounded-xl aspect-[4/5] sm:aspect-square md:aspect-[4/3] max-w-[500px] mx-auto border border-border-subtle/50 bg-black/10">
                            {/* SVG Lines */}
                            <svg className="absolute inset-0 w-full h-full pointer-events-none" style={{ zIndex: 0 }}>
                                <style>
                                    {`
                                        .dash-anim-fwd {
                                            stroke-dasharray: 6 6;
                                            animation: dash-fw 1.5s linear infinite;
                                        }
                                        @keyframes dash-fw { from { stroke-dashoffset: 12; } to { stroke-dashoffset: 0; } }
                                        
                                        @keyframes spin-gradient {
                                            0% { transform: rotate(0deg); }
                                            100% { transform: rotate(360deg); }
                                        }
                                    `}
                                </style>
                                {/* Horizontal connecting lines (Top) */}
                                <line x1="25%" y1="calc(25% - 6px)" x2="75%" y2="calc(25% - 6px)" stroke="#d3c8a8" strokeWidth="2" strokeOpacity="0.6" className="dash-anim-fwd" />
                                <line x1="75%" y1="calc(25% + 6px)" x2="25%" y2="calc(25% + 6px)" stroke="#666666" strokeWidth="2" strokeOpacity="0.4" className="dash-anim-fwd" />

                                {/* Vertical connecting lines (Left) */}
                                <line x1="calc(25% - 6px)" y1="25%" x2="calc(25% - 6px)" y2="75%" stroke="#666666" strokeWidth="2" strokeOpacity="0.4" className="dash-anim-fwd" />
                                <line x1="calc(25% + 6px)" y1="75%" x2="calc(25% + 6px)" y2="25%" stroke="#d3c8a8" strokeWidth="2" strokeOpacity="0.6" className="dash-anim-fwd" />
                            </svg>

                            <div className="absolute inset-0 grid grid-cols-2 grid-rows-2 z-10">
                                {/* Top Left: User Machine */}
                                <div className="flex justify-center items-center relative">
                                    <div className="absolute inset-0 bg-accent-gold/5 rounded-full blur-xl scale-50 pointer-events-none" />
                                    <div className="flex flex-col items-center bg-bg-surface/80 backdrop-blur border border-border-subtle p-3 lg:p-4 rounded-xl shadow-lg w-32 md:w-40 text-center">
                                        <Monitor className="text-text-primary mb-2 w-5 h-5 md:w-6 md:h-6" />
                                        <h3 className="text-xs md:text-sm font-semibold text-text-primary mb-2">User Machine</h3>
                                        <div className="text-[9px] md:text-[10px] text-text-muted flex flex-col gap-0.5">
                                            <span>Game Client</span>
                                            <span>TightVNC Server</span>
                                            <span>EasyAntiCheat</span>
                                        </div>
                                    </div>
                                </div>

                                {/* Top Right: Game Server */}
                                <div className="flex justify-center items-center relative group/box">
                                    <div className="absolute inset-0 bg-border-strong/10 rounded-full blur-xl scale-50 group-hover/box:scale-110 transition-transform duration-500 pointer-events-none" />
                                    <div className="flex flex-col items-center bg-bg-surface/80 backdrop-blur border border-border-subtle p-3 lg:p-4 rounded-xl shadow-lg w-32 md:w-40 text-center">
                                        <Server className="text-text-primary mb-2 w-5 h-5 md:w-6 md:h-6" />
                                        <h3 className="text-xs md:text-sm font-semibold text-text-primary mb-2">Game Server</h3>
                                        <p className="text-[9px] md:text-[10px] text-text-muted mt-1 px-1 leading-tight">
                                            All traffic comes straight from your local network
                                        </p>
                                    </div>
                                </div>

                                {/* Bottom Left: Oryxbot Cloud */}
                                <div className="flex justify-center items-center relative group/box">
                                    <div className="flex flex-col items-center bg-[#1a1c1a]/90 backdrop-blur-md border border-accent-gold/40 p-3 lg:p-4 rounded-xl shadow-[0_0_20px_rgba(211,200,168,0.15)] w-32 md:w-40 text-center overflow-hidden">
                                        <div className="absolute top-0 inset-x-0 h-1/2 bg-[radial-gradient(ellipse_at_top,rgba(211,200,168,0.2),transparent_70%)] pointer-events-none" />

                                        <Cloud className="text-accent-gold mb-2 w-5 h-5 md:w-6 md:h-6 relative z-10" />
                                        <h3 className="text-xs md:text-sm font-semibold text-gradient-gold mb-2 relative z-10 leading-tight">Oryxbot Cloud</h3>
                                        <div className="text-[9px] md:text-[10px] text-text-muted flex flex-col gap-0.5 relative z-10">
                                            <span>Cheat software</span>
                                            <span>Packet Sniffer</span>
                                            <span>TightVNC Client</span>
                                        </div>
                                    </div>
                                </div>

                                {/* Bottom Right: Spacer */}
                            </div>
                        </div>

                        {/* Powered By Link */}
                        <div className="absolute bottom-4 sm:bottom-6 right-4 sm:right-6 z-20">
                            <a
                                href="https://railrip.com"
                                target="_blank"
                                rel="noopener noreferrer"
                                className="group flex items-center gap-1.5 px-3 py-1.5 rounded-full bg-bg-surface/80 border border-border-subtle hover:border-accent-gold/40 backdrop-blur-md transition-all duration-300 shadow-lg"
                            >
                                <span className="text-[10px] text-text-muted group-hover:text-text-primary transition-colors">Powered by</span>
                                <span className="text-[10px] sm:text-xs font-semibold text-gradient-gold">railrip.com</span>
                            </a>
                        </div>
                    </div>
                </motion.div>

            </div>
        </section>
    )
}
