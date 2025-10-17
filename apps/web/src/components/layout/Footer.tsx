export default function Footer() {
  return (
    <footer className="bg-white border-t border-gray-200 mt-auto">
      <div className="container mx-auto px-4 py-6">
        <div className="flex items-center justify-between text-sm text-gray-600">
          <p>&copy; 2025 LLM Pricing Comparison. All rights reserved.</p>
          <div className="flex gap-4">
            <a href="#" className="hover:text-gray-900 transition-colors">
              About
            </a>
            <a href="#" className="hover:text-gray-900 transition-colors">
              Documentation
            </a>
            <a href="#" className="hover:text-gray-900 transition-colors">
              GitHub
            </a>
          </div>
        </div>
      </div>
    </footer>
  )
}
