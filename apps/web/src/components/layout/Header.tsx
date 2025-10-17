import { Link } from 'react-router-dom'

export default function Header() {
  return (
    <header className="bg-white border-b border-gray-200 sticky top-0 z-10">
      <div className="container mx-auto px-4">
        <div className="flex items-center justify-between h-16">
          <Link to="/" className="text-xl font-bold text-gray-900">
            LLM Pricing
          </Link>

          <nav className="flex gap-6">
            <Link
              to="/"
              className="text-gray-600 hover:text-gray-900 transition-colors"
            >
              Home
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
        </div>
      </div>
    </header>
  )
}
