import { Link } from 'react-router-dom'
import { Search } from 'lucide-react'

export default function Header() {
  return (
    <header className="bg-white border-b border-gray-200 sticky top-0 z-10">
      <div className="container mx-auto px-4">
        <div className="flex items-center justify-between gap-4 h-16">
          {/* Logo/Brand */}
          <Link
            to="/"
            className="text-xl font-bold text-gray-900 flex-shrink-0"
            aria-label="LLM Pricing - Home"
          >
            LLM Pricing
          </Link>

          {/* Search Input Placeholder (Story 3.1 AC #2) */}
          <div className="hidden md:flex flex-1 max-w-md">
            <div className="relative w-full">
              <Search
                className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-gray-400"
                aria-hidden="true"
              />
              <input
                type="search"
                placeholder="Search models..."
                aria-label="Search models"
                className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-md text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                disabled
                title="Search functionality coming soon"
              />
            </div>
          </div>

          {/* Navigation (Story 3.1 AC #14 - ARIA labels) */}
          <nav aria-label="Main navigation" className="hidden sm:flex gap-6">
            <Link
              to="/"
              className="text-gray-600 hover:text-gray-900 transition-colors"
              aria-label="Home page"
            >
              Home
            </Link>
            <Link
              to="/calculator"
              className="text-gray-600 hover:text-gray-900 transition-colors"
              aria-label="Cost calculator"
            >
              Calculator
            </Link>
            <Link
              to="/compare"
              className="text-gray-600 hover:text-gray-900 transition-colors"
              aria-label="Compare models"
            >
              Compare
            </Link>
          </nav>

          {/* Mobile Menu Button Placeholder */}
          <button
            type="button"
            className="sm:hidden p-2 text-gray-600 hover:text-gray-900"
            aria-label="Open navigation menu"
            aria-expanded="false"
            disabled
            title="Mobile menu coming soon"
          >
            <svg
              className="h-6 w-6"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
              aria-hidden="true"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M4 6h16M4 12h16M4 18h16"
              />
            </svg>
          </button>
        </div>
      </div>
    </header>
  )
}
