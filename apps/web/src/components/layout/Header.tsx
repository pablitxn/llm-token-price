import { Link } from 'react-router-dom'

export default function Header() {
  return (
    <header className="bg-white border-b border-gray-200 sticky top-0 z-10">
      <div className="container mx-auto px-4">
        <div className="flex items-center justify-between h-16 gap-8">
          {/* Logo/Brand */}
          <Link to="/" className="text-xl font-bold text-gray-900 whitespace-nowrap">
            LLM Pricing
          </Link>

          {/* Navigation Links */}
          <nav className="hidden md:flex gap-6">
            <Link
              to="/"
              className="text-gray-600 hover:text-gray-900 transition-colors"
            >
              Models
            </Link>
            <Link
              to="/calculator"
              className="text-gray-600 hover:text-gray-900 transition-colors"
            >
              Calculator
            </Link>
            <Link
              to="/compare"
              className="text-gray-600 hover:text-gray-900 transition-colors"
            >
              Compare
            </Link>
          </nav>

          {/* Search Input Placeholder */}
          <div className="hidden md:flex flex-1 max-w-md">
            <input
              type="text"
              placeholder="Search models..."
              disabled
              className="w-full px-4 py-2 border border-gray-300 rounded-lg bg-gray-50 text-gray-500 cursor-not-allowed"
              aria-label="Search models (coming soon)"
            />
          </div>

          {/* Admin Link */}
          <Link
            to="/admin"
            className="text-gray-600 hover:text-gray-900 transition-colors whitespace-nowrap"
          >
            Admin
          </Link>
        </div>
      </div>
    </header>
  )
}
