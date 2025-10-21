import { useState, useRef, useEffect } from 'react'
import { Menu, LogOut, User } from 'lucide-react'
import { useAuth } from '@/hooks/useAuth.ts'
import { LanguageSelector } from '@/components/admin/LanguageSelector'

/**
 * Props for AdminHeader component
 */
interface AdminHeaderProps {
  /** Callback when hamburger menu is clicked (mobile/tablet) */
  onMenuClick: () => void
}

/**
 * AdminHeader Component
 *
 * Top navigation bar for admin panel with:
 * - Hamburger menu button (mobile/tablet only)
 * - Admin Panel title
 * - User dropdown with username and logout button
 *
 * Features:
 * - Displays logged-in admin username from auth store
 * - Dropdown menu with click-outside detection
 * - Logout functionality via useAuth hook
 * - Responsive behavior (hamburger hidden on desktop)
 *
 * @component
 */
export default function AdminHeader({ onMenuClick }: AdminHeaderProps) {
  const { user, logout } = useAuth()
  const [isDropdownOpen, setIsDropdownOpen] = useState(false)
  const dropdownRef = useRef<HTMLDivElement>(null)

  // Close dropdown when clicking outside
  useEffect(() => {
    function handleClickOutside(event: MouseEvent) {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
        setIsDropdownOpen(false)
      }
    }

    if (isDropdownOpen) {
      document.addEventListener('mousedown', handleClickOutside)
      return () => document.removeEventListener('mousedown', handleClickOutside)
    }
  }, [isDropdownOpen])

  const handleLogout = () => {
    setIsDropdownOpen(false)
    logout()
  }

  return (
    <header className="h-16 bg-white border-b border-slate-200 flex items-center justify-between px-6 sticky top-0 z-20">
      {/* Left Section: Hamburger + Title */}
      <div className="flex items-center gap-4">
        {/* Hamburger Menu (Mobile/Tablet Only) */}
        <button
          onClick={onMenuClick}
          className="lg:hidden p-2 hover:bg-slate-100 rounded-md transition-colors"
          aria-label="Toggle menu"
        >
          <Menu className="w-6 h-6 text-slate-700" />
        </button>

        {/* Title */}
        <h2 className="text-lg font-semibold text-slate-900">Admin Panel</h2>
      </div>

      {/* Right Section: Language Selector + User Dropdown */}
      <div className="flex items-center gap-3">
        {/* Language Selector (Story 2.13 Task 13.6) */}
        <LanguageSelector />

        {/* User Dropdown */}
        <div className="relative" ref={dropdownRef}>
        <button
          onClick={() => setIsDropdownOpen(!isDropdownOpen)}
          className="flex items-center gap-2 px-3 py-2 hover:bg-slate-100 rounded-md transition-colors"
          aria-expanded={isDropdownOpen}
          aria-haspopup="true"
        >
          <div className="w-8 h-8 bg-slate-700 rounded-full flex items-center justify-center">
            <User className="w-5 h-5 text-white" />
          </div>
          <span className="text-sm font-medium text-slate-900 hidden sm:block">
            {user?.username || 'Admin'}
          </span>
        </button>

        {/* Dropdown Menu */}
        {isDropdownOpen && (
          <div className="absolute right-0 mt-2 w-48 bg-white border border-slate-200 rounded-lg shadow-lg py-1">
            {/* User Info */}
            <div className="px-4 py-2 border-b border-slate-100">
              <p className="text-sm font-medium text-slate-900">{user?.username || 'Admin'}</p>
              <p className="text-xs text-slate-500">{user?.role || 'Administrator'}</p>
            </div>

            {/* Logout Button */}
            <button
              onClick={handleLogout}
              className="w-full flex items-center gap-2 px-4 py-2 text-sm text-red-600 hover:bg-red-50 transition-colors"
            >
              <LogOut className="w-4 h-4" />
              <span>Logout</span>
            </button>
          </div>
        )}
        </div>
      </div>
    </header>
  )
}
