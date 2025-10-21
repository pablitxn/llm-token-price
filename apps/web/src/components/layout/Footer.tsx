export default function Footer() {
  const currentYear = new Date().getFullYear()

  return (
    <footer className="bg-white border-t border-gray-200 mt-auto">
      <div className="container mx-auto px-4 py-6">
        <div className="flex flex-col sm:flex-row items-center justify-between gap-4 text-sm text-gray-600">
          <p>&copy; {currentYear} LLM Pricing Comparison. All rights reserved.</p>
          <div className="flex gap-4">
            <a href="#about" className="hover:text-gray-900 transition-colors">
              About
            </a>
            <a href="#contact" className="hover:text-gray-900 transition-colors">
              Contact
            </a>
          </div>
        </div>
      </div>
    </footer>
  )
}
