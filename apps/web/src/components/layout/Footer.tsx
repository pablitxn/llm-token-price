export default function Footer() {
  return (
    <footer className="bg-white border-t border-gray-200 mt-auto">
      <div className="container mx-auto px-4 py-6">
        <div className="flex flex-col sm:flex-row items-center justify-between gap-4 text-sm text-gray-600">
          <p>&copy; 2025 LLM Pricing Comparison. All rights reserved.</p>
          <nav aria-label="Footer navigation" className="flex gap-4">
            <a
              href="#about"
              className="hover:text-gray-900 transition-colors"
              aria-label="About page"
            >
              About
            </a>
            <a
              href="#contact"
              className="hover:text-gray-900 transition-colors"
              aria-label="Contact page"
            >
              Contact
            </a>
            <a
              href="#documentation"
              className="hover:text-gray-900 transition-colors"
              aria-label="Documentation"
            >
              Documentation
            </a>
            <a
              href="https://github.com"
              className="hover:text-gray-900 transition-colors"
              aria-label="GitHub repository"
              target="_blank"
              rel="noopener noreferrer"
            >
              GitHub
            </a>
          </nav>
        </div>
      </div>
    </footer>
  )
}
