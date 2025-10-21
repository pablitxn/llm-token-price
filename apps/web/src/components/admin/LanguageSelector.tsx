import { useState, useEffect } from 'react';
import { Globe } from 'lucide-react';

/**
 * Language codes supported by the API backend.
 * Must match the cultures configured in ASP.NET Core RequestLocalizationOptions.
 */
type SupportedLanguage = 'en' | 'es';

interface LanguageOption {
  code: SupportedLanguage;
  label: string;
  nativeLabel: string;
}

const LANGUAGE_OPTIONS: LanguageOption[] = [
  { code: 'en', label: 'English', nativeLabel: 'English' },
  { code: 'es', label: 'Spanish', nativeLabel: 'Español' },
];

const STORAGE_KEY = 'llm-pricing-language';

/**
 * LanguageSelector Component
 *
 * Provides a dropdown to switch between English and Spanish for the admin panel.
 * Stores the user's language preference in localStorage and automatically sends
 * the Accept-Language header with all API requests.
 *
 * Architecture:
 * - Language preference stored in localStorage (persists across sessions)
 * - axios interceptor configured in api/client.ts reads from localStorage
 * - Backend RequestLocalizationMiddleware reads Accept-Language header
 * - FluentValidation error messages automatically returned in selected language
 *
 * Usage:
 * ```tsx
 * <LanguageSelector />
 * ```
 */
export function LanguageSelector() {
  const [selectedLanguage, setSelectedLanguage] = useState<SupportedLanguage>(() => {
    // Initialize from localStorage or default to English
    const stored = localStorage.getItem(STORAGE_KEY) as SupportedLanguage | null;
    return stored && ['en', 'es'].includes(stored) ? stored : 'en';
  });

  const [isOpen, setIsOpen] = useState(false);

  useEffect(() => {
    // Persist language preference to localStorage
    localStorage.setItem(STORAGE_KEY, selectedLanguage);

    // Dispatch custom event to notify API client to update Accept-Language header
    window.dispatchEvent(new CustomEvent('language-changed', {
      detail: { language: selectedLanguage }
    }));
  }, [selectedLanguage]);

  const handleLanguageChange = (language: SupportedLanguage) => {
    setSelectedLanguage(language);
    setIsOpen(false);
  };

  const currentLanguage = LANGUAGE_OPTIONS.find(lang => lang.code === selectedLanguage);

  return (
    <div className="relative">
      {/* Language Selector Button */}
      <button
        onClick={() => setIsOpen(!isOpen)}
        className="flex items-center gap-2 px-3 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
        aria-label="Select language"
        aria-expanded={isOpen}
        aria-haspopup="true"
      >
        <Globe className="w-4 h-4" />
        <span>{currentLanguage?.nativeLabel}</span>
        <svg
          className={`w-4 h-4 transition-transform ${isOpen ? 'rotate-180' : ''}`}
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
        >
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
        </svg>
      </button>

      {/* Dropdown Menu */}
      {isOpen && (
        <>
          {/* Backdrop to close dropdown when clicking outside */}
          <div
            className="fixed inset-0 z-10"
            onClick={() => setIsOpen(false)}
            aria-hidden="true"
          />

          {/* Dropdown Panel */}
          <div className="absolute right-0 z-20 mt-2 w-48 bg-white border border-gray-200 rounded-md shadow-lg">
            <div className="py-1" role="menu" aria-orientation="vertical">
              {LANGUAGE_OPTIONS.map((language) => (
                <button
                  key={language.code}
                  onClick={() => handleLanguageChange(language.code)}
                  className={`w-full text-left px-4 py-2 text-sm ${
                    selectedLanguage === language.code
                      ? 'bg-blue-50 text-blue-700 font-medium'
                      : 'text-gray-700 hover:bg-gray-100'
                  }`}
                  role="menuitem"
                >
                  <div className="flex items-center justify-between">
                    <span>{language.nativeLabel}</span>
                    {selectedLanguage === language.code && (
                      <svg
                        className="w-4 h-4 text-blue-600"
                        fill="currentColor"
                        viewBox="0 0 20 20"
                      >
                        <path
                          fillRule="evenodd"
                          d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z"
                          clipRule="evenodd"
                        />
                      </svg>
                    )}
                  </div>
                </button>
              ))}
            </div>
          </div>
        </>
      )}
    </div>
  );
}

/**
 * Hook to get the current language from localStorage.
 * Useful for components that need to react to language changes.
 *
 * @returns Current language code ('en' or 'es')
 */
export function useLanguage(): SupportedLanguage {
  const [language, setLanguage] = useState<SupportedLanguage>(() => {
    const stored = localStorage.getItem(STORAGE_KEY) as SupportedLanguage | null;
    return stored && ['en', 'es'].includes(stored) ? stored : 'en';
  });

  useEffect(() => {
    const handleLanguageChange = (event: CustomEvent<{ language: SupportedLanguage }>) => {
      setLanguage(event.detail.language);
    };

    window.addEventListener('language-changed', handleLanguageChange as EventListener);
    return () => {
      window.removeEventListener('language-changed', handleLanguageChange as EventListener);
    };
  }, []);

  return language;
}

/**
 * Get the current language from localStorage (synchronous).
 * Used by axios interceptor to set Accept-Language header.
 *
 * @returns Current language code ('en' or 'es')
 */
export function getCurrentLanguage(): SupportedLanguage {
  const stored = localStorage.getItem(STORAGE_KEY) as SupportedLanguage | null;
  return stored && ['en', 'es'].includes(stored) ? stored : 'en';
}
