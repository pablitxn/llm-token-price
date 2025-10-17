import { NavLink } from 'react-router-dom'
import { LayoutDashboard, Database, BarChart, type LucideIcon } from 'lucide-react'

/**
 * Navigation item structure
 */
interface NavItem {
  name: string
  icon: LucideIcon
  path: string
}

/**
 * Navigation items for admin panel
 */
const navItems: NavItem[] = [
  {
    name: 'Dashboard',
    icon: LayoutDashboard,
    path: '/admin/dashboard',
  },
  {
    name: 'Models',
    icon: Database,
    path: '/admin/models',
  },
  {
    name: 'Benchmarks',
    icon: BarChart,
    path: '/admin/benchmarks',
  },
]

/**
 * Props for AdminSidebar component
 */
interface AdminSidebarProps {
  /** Whether sidebar is open (mobile/tablet) */
  isOpen: boolean

  /** Callback to close sidebar (mobile/tablet) */
  onClose: () => void
}

/**
 * AdminSidebar Component
 *
 * Responsive navigation sidebar with:
 * - Desktop (≥1024px): Fixed position, always visible
 * - Mobile/Tablet (<1024px): Slide-in drawer pattern
 *
 * Features:
 * - Active route highlighting via NavLink
 * - Lucide React icons
 * - Smooth transitions
 * - Touch-friendly tap targets (min 44px)
 *
 * @component
 */
export default function AdminSidebar({ isOpen, onClose }: AdminSidebarProps) {
  return (
    <aside
      className={`
        fixed top-0 left-0 h-full w-64 bg-slate-800 text-white
        transition-transform duration-300 ease-in-out z-40
        ${isOpen ? 'translate-x-0' : '-translate-x-full'}
        lg:translate-x-0
      `}
    >
      {/* Sidebar Header */}
      <div className="h-16 flex items-center px-6 border-b border-slate-700">
        <h1 className="text-xl font-bold">LLM Admin</h1>
      </div>

      {/* Navigation Menu */}
      <nav className="mt-6 px-3">
        {navItems.map((item) => (
          <NavLink
            key={item.path}
            to={item.path}
            onClick={onClose} // Close drawer on mobile after navigation
            className={({ isActive }) =>
              `
              flex items-center gap-3 px-3 py-3 rounded-lg mb-2
              transition-colors duration-200
              ${
                isActive
                  ? 'bg-slate-700 text-white font-medium'
                  : 'text-slate-300 hover:bg-slate-700/50 hover:text-white'
              }
            `
            }
          >
            {({ isActive }) => (
              <>
                <item.icon
                  className={`w-5 h-5 ${isActive ? 'text-blue-400' : 'text-slate-400'}`}
                />
                <span>{item.name}</span>
              </>
            )}
          </NavLink>
        ))}
      </nav>

      {/* Sidebar Footer (optional - version info, etc.) */}
      <div className="absolute bottom-0 left-0 right-0 p-4 border-t border-slate-700">
        <p className="text-xs text-slate-500 text-center">Admin Panel v1.0</p>
      </div>
    </aside>
  )
}
