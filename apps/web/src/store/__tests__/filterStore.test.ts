import { renderHook, act } from '@testing-library/react'
import { useFilterStore } from '../filterStore'

/**
 * Test suite for filterStore (Story 3.5: Task 7)
 *
 * CRITICAL: First global Zustand store in Epic 3 - validates foundational state
 * management pattern for Stories 3.6-3.7 and 3.11
 *
 * Subtasks tested:
 * 7.1: toggleProvider adds provider when not present
 * 7.2: toggleProvider removes provider when already present
 * 7.3: clearFilters resets to empty array
 * 7.4: getActiveFilterCount returns correct count
 * 7.5: Store persists across component remounts
 * 7.6: Multiple components read same state (singleton pattern)
 * 7.7: State updates trigger re-renders
 * 7.8: Initial state is empty array
 */
describe('filterStore', () => {
  // Reset store before each test to ensure isolation
  beforeEach(() => {
    const { result } = renderHook(() => useFilterStore())
    act(() => {
      result.current.clearFilters()
    })
  })

  describe('Subtask 7.8: Initial state', () => {
    it('should have empty selectedProviders array on first load', () => {
      const { result } = renderHook(() => useFilterStore())

      expect(result.current.selectedProviders).toEqual([])
      expect(result.current.getActiveFilterCount()).toBe(0)
    })
  })

  describe('Subtask 7.1: toggleProvider adds provider', () => {
    it('should add provider to selectedProviders when not present', () => {
      const { result } = renderHook(() => useFilterStore())

      act(() => {
        result.current.toggleProvider('OpenAI')
      })

      expect(result.current.selectedProviders).toContain('OpenAI')
      expect(result.current.selectedProviders.length).toBe(1)
    })

    it('should add multiple different providers', () => {
      const { result } = renderHook(() => useFilterStore())

      act(() => {
        result.current.toggleProvider('OpenAI')
        result.current.toggleProvider('Anthropic')
        result.current.toggleProvider('Google')
      })

      expect(result.current.selectedProviders).toEqual(['OpenAI', 'Anthropic', 'Google'])
      expect(result.current.selectedProviders.length).toBe(3)
    })
  })

  describe('Subtask 7.2: toggleProvider removes provider', () => {
    it('should remove provider from selectedProviders when already present', () => {
      const { result } = renderHook(() => useFilterStore())

      // Add provider first
      act(() => {
        result.current.toggleProvider('OpenAI')
      })
      expect(result.current.selectedProviders).toContain('OpenAI')

      // Remove provider
      act(() => {
        result.current.toggleProvider('OpenAI')
      })
      expect(result.current.selectedProviders).not.toContain('OpenAI')
      expect(result.current.selectedProviders.length).toBe(0)
    })

    it('should remove only the specified provider', () => {
      const { result } = renderHook(() => useFilterStore())

      // Add multiple providers
      act(() => {
        result.current.toggleProvider('OpenAI')
        result.current.toggleProvider('Anthropic')
        result.current.toggleProvider('Google')
      })

      // Remove middle provider
      act(() => {
        result.current.toggleProvider('Anthropic')
      })

      expect(result.current.selectedProviders).toEqual(['OpenAI', 'Google'])
      expect(result.current.selectedProviders).not.toContain('Anthropic')
    })
  })

  describe('Subtask 7.3: clearFilters resets to empty array', () => {
    it('should reset selectedProviders to empty array', () => {
      const { result } = renderHook(() => useFilterStore())

      // Add multiple providers
      act(() => {
        result.current.toggleProvider('OpenAI')
        result.current.toggleProvider('Anthropic')
        result.current.toggleProvider('Google')
      })
      expect(result.current.selectedProviders.length).toBe(3)

      // Clear filters
      act(() => {
        result.current.clearFilters()
      })

      expect(result.current.selectedProviders).toEqual([])
      expect(result.current.selectedProviders.length).toBe(0)
    })

    it('should work correctly when called on already empty state', () => {
      const { result } = renderHook(() => useFilterStore())

      expect(result.current.selectedProviders).toEqual([])

      // Clear when already empty
      act(() => {
        result.current.clearFilters()
      })

      expect(result.current.selectedProviders).toEqual([])
    })
  })

  describe('Subtask 7.4: getActiveFilterCount returns correct count', () => {
    it('should return 0 when empty', () => {
      const { result } = renderHook(() => useFilterStore())

      expect(result.current.getActiveFilterCount()).toBe(0)
    })

    it('should return correct count with 1 provider', () => {
      const { result } = renderHook(() => useFilterStore())

      act(() => {
        result.current.toggleProvider('OpenAI')
      })

      expect(result.current.getActiveFilterCount()).toBe(1)
    })

    it('should return correct count with 2 providers', () => {
      const { result } = renderHook(() => useFilterStore())

      act(() => {
        result.current.toggleProvider('OpenAI')
        result.current.toggleProvider('Anthropic')
      })

      expect(result.current.getActiveFilterCount()).toBe(2)
    })

    it('should return correct count with 3 providers', () => {
      const { result } = renderHook(() => useFilterStore())

      act(() => {
        result.current.toggleProvider('OpenAI')
        result.current.toggleProvider('Anthropic')
        result.current.toggleProvider('Google')
      })

      expect(result.current.getActiveFilterCount()).toBe(3)
    })

    it('should update count after removing a provider', () => {
      const { result } = renderHook(() => useFilterStore())

      act(() => {
        result.current.toggleProvider('OpenAI')
        result.current.toggleProvider('Anthropic')
      })
      expect(result.current.getActiveFilterCount()).toBe(2)

      act(() => {
        result.current.toggleProvider('OpenAI')
      })
      expect(result.current.getActiveFilterCount()).toBe(1)
    })
  })

  describe('Subtask 7.5: Store persists across component remounts', () => {
    it('should maintain state after hook unmount and remount', () => {
      // First mount
      const { result: result1, unmount } = renderHook(() => useFilterStore())

      act(() => {
        result1.current.toggleProvider('OpenAI')
        result1.current.toggleProvider('Anthropic')
      })

      expect(result1.current.selectedProviders).toEqual(['OpenAI', 'Anthropic'])

      // Unmount first hook
      unmount()

      // Remount with new hook instance
      const { result: result2 } = renderHook(() => useFilterStore())

      // State should persist (Zustand singleton pattern)
      expect(result2.current.selectedProviders).toEqual(['OpenAI', 'Anthropic'])
      expect(result2.current.getActiveFilterCount()).toBe(2)
    })
  })

  describe('Subtask 7.6: Multiple components read same state (singleton pattern)', () => {
    it('should share state between multiple hook instances', () => {
      const { result: hook1 } = renderHook(() => useFilterStore())
      const { result: hook2 } = renderHook(() => useFilterStore())

      // Modify state via hook1
      act(() => {
        hook1.current.toggleProvider('OpenAI')
      })

      // Both hooks should see the same state
      expect(hook1.current.selectedProviders).toContain('OpenAI')
      expect(hook2.current.selectedProviders).toContain('OpenAI')
      expect(hook1.current.selectedProviders).toEqual(hook2.current.selectedProviders)
    })

    it('should maintain singleton state across multiple modifications', () => {
      const { result: hook1 } = renderHook(() => useFilterStore())
      const { result: hook2 } = renderHook(() => useFilterStore())
      const { result: hook3 } = renderHook(() => useFilterStore())

      // Modify via different hooks
      act(() => {
        hook1.current.toggleProvider('OpenAI')
      })

      act(() => {
        hook2.current.toggleProvider('Anthropic')
      })

      act(() => {
        hook3.current.toggleProvider('Google')
      })

      // All hooks should have all three providers
      const expected = ['OpenAI', 'Anthropic', 'Google']
      expect(hook1.current.selectedProviders).toEqual(expected)
      expect(hook2.current.selectedProviders).toEqual(expected)
      expect(hook3.current.selectedProviders).toEqual(expected)
    })
  })

  describe('Subtask 7.7: State updates trigger re-renders', () => {
    it('should trigger re-render when toggleProvider is called', () => {
      const { result, rerender } = renderHook(() => useFilterStore())

      const initialRender = result.current.selectedProviders

      act(() => {
        result.current.toggleProvider('OpenAI')
      })

      // State should have changed, triggering re-render
      expect(result.current.selectedProviders).not.toBe(initialRender)
      expect(result.current.selectedProviders).toContain('OpenAI')
    })

    it('should trigger re-render when clearFilters is called', () => {
      const { result } = renderHook(() => useFilterStore())

      act(() => {
        result.current.toggleProvider('OpenAI')
        result.current.toggleProvider('Anthropic')
      })

      const beforeClear = result.current.selectedProviders
      expect(beforeClear.length).toBe(2)

      act(() => {
        result.current.clearFilters()
      })

      // State should have changed
      expect(result.current.selectedProviders).not.toBe(beforeClear)
      expect(result.current.selectedProviders).toEqual([])
    })
  })

  describe('Subtask 7.4 (Additional): Stress test for rapid toggleProvider calls', () => {
    it('should handle rapid sequential toggleProvider calls correctly', () => {
      const { result } = renderHook(() => useFilterStore())

      // Rapidly toggle providers
      act(() => {
        result.current.toggleProvider('OpenAI')
        result.current.toggleProvider('Anthropic')
        result.current.toggleProvider('Google')
        result.current.toggleProvider('OpenAI') // Remove
        result.current.toggleProvider('Mistral')
        result.current.toggleProvider('Anthropic') // Remove
        result.current.toggleProvider('Cohere')
      })

      // Final state should be: Google, Mistral, Cohere (OpenAI and Anthropic removed)
      expect(result.current.selectedProviders).toEqual(['Google', 'Mistral', 'Cohere'])
      expect(result.current.getActiveFilterCount()).toBe(3)
    })

    it('should handle rapid toggle of same provider', () => {
      const { result } = renderHook(() => useFilterStore())

      // Rapidly toggle same provider multiple times
      act(() => {
        result.current.toggleProvider('OpenAI') // Add
        result.current.toggleProvider('OpenAI') // Remove
        result.current.toggleProvider('OpenAI') // Add
        result.current.toggleProvider('OpenAI') // Remove
        result.current.toggleProvider('OpenAI') // Add
      })

      // Should end with OpenAI added (odd number of toggles)
      expect(result.current.selectedProviders).toEqual(['OpenAI'])
      expect(result.current.getActiveFilterCount()).toBe(1)
    })
  })
})
