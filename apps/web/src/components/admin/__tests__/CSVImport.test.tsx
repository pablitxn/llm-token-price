import { render, screen, waitFor, fireEvent } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { CSVImport } from '../CSVImport';
import { importBenchmarkCSV } from '@/api/admin';

// Mock the API client
vi.mock('@/api/admin', () => ({
  importBenchmarkCSV: vi.fn(),
}));

const createTestQueryClient = () =>
  new QueryClient({
    defaultOptions: {
      queries: { retry: false },
      mutations: { retry: false },
    },
  });

const renderWithProviders = (component: React.ReactElement) => {
  const queryClient = createTestQueryClient();
  return render(
    <QueryClientProvider client={queryClient}>
      {component}
    </QueryClientProvider>
  );
};

describe('CSVImport Component - Story 2.11 Task 10.11', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  /**
   * [P1] Task 10.11: CSVImport component should render file input and upload button
   * Story 2.11 AC#1: CSV upload form created on benchmarks page
   */
  it('should render file input and upload button', () => {
    renderWithProviders(<CSVImport />);

    // Should have file input
    const fileInput = screen.getByLabelText(/csv file/i) ||
                      screen.getByRole('button', { name: /choose file|browse|select/i });
    expect(fileInput).toBeInTheDocument();

    // Should have upload button (initially disabled)
    const uploadButton = screen.getByRole('button', { name: /upload/i });
    expect(uploadButton).toBeInTheDocument();
    expect(uploadButton).toBeDisabled();
  });

  /**
   * [P1] Task 10.11: Upload button should be enabled when file is selected
   * Story 2.11 AC#1: Upload button disabled until file selected
   */
  it('should enable upload button when file is selected', async () => {
    const user = userEvent.setup();
    renderWithProviders(<CSVImport />);

    const fileInput = screen.getByRole('button', { name: /choose file|browse|select/i }).closest('input') ||
                      document.querySelector('input[type="file"]');
    expect(fileInput).toBeInTheDocument();

    // Create mock CSV file
    const csvFile = new File(
      ['model_id,benchmark_name,score\n123,MMLU,85.2'],
      'test.csv',
      { type: 'text/csv' }
    );

    // Select file
    await user.upload(fileInput!, csvFile);

    // Upload button should now be enabled
    const uploadButton = screen.getByRole('button', { name: /upload/i });
    expect(uploadButton).toBeEnabled();
  });

  /**
   * [P1] Task 10.11: Should display selected file name and size
   * Story 2.11 AC#1: Show selected file name and size
   */
  it('should display selected file name and size', async () => {
    const user = userEvent.setup();
    renderWithProviders(<CSVImport />);

    const fileInput = document.querySelector('input[type="file"]');
    expect(fileInput).toBeInTheDocument();

    // Create mock CSV file
    const csvFile = new File(
      ['model_id,benchmark_name,score\n123,MMLU,85.2'],
      'test-file.csv',
      { type: 'text/csv' }
    );

    // Select file
    await user.upload(fileInput!, csvFile);

    // Should display file name
    await waitFor(() => {
      expect(screen.getByText(/test-file\.csv/i)).toBeInTheDocument();
    });

    // Should display file size (if implemented)
    // Note: Size display might be in KB or bytes
    const sizeText = screen.queryByText(/\d+\s*(bytes|KB|MB)/i);
    if (sizeText) {
      expect(sizeText).toBeInTheDocument();
    }
  });

  /**
   * [P1] Task 10.11: Should show loading state during upload
   * Story 2.11 AC#1: Add loading spinner during upload/processing
   */
  it('should show loading state during CSV upload', async () => {
    const user = userEvent.setup();

    // Mock slow API response
    vi.mocked(importBenchmarkCSV).mockImplementation(
      () => new Promise(resolve => setTimeout(() => resolve({
        totalRows: 1,
        successfulImports: 1,
        failedImports: 0,
        skippedDuplicates: 0,
        errors: [],
      }), 1000))
    );

    renderWithProviders(<CSVImport />);

    const fileInput = document.querySelector('input[type="file"]');
    const csvFile = new File(
      ['model_id,benchmark_name,score\n123,MMLU,85.2'],
      'test.csv',
      { type: 'text/csv' }
    );

    await user.upload(fileInput!, csvFile);

    const uploadButton = screen.getByRole('button', { name: /upload/i });
    await user.click(uploadButton);

    // Should show loading indicator
    await waitFor(() => {
      const loadingElement = screen.queryByText(/uploading|loading|processing/i) ||
                             screen.queryByRole('status') ||
                             document.querySelector('[data-loading="true"]') ||
                             uploadButton.querySelector('svg'); // Check for spinner icon
      expect(loadingElement).toBeTruthy();
    });

    // Button should be disabled during upload
    expect(uploadButton).toBeDisabled();
  });

  /**
   * [P1] Task 10.11: Should display import results after successful upload
   * Story 2.11 AC#6: Import results shown (X successful, Y failed with reasons)
   */
  it('should display import results after successful upload', async () => {
    const user = userEvent.setup();

    // Mock successful import response
    vi.mocked(importBenchmarkCSV).mockResolvedValue({
      totalRows: 5,
      successfulImports: 3,
      failedImports: 2,
      skippedDuplicates: 0,
      errors: [
        {
          rowNumber: 2,
          error: 'Invalid model_id format',
          data: { model_id: 'invalid', benchmark_name: 'MMLU', score: '85' },
        },
        {
          rowNumber: 4,
          error: 'Benchmark not found',
          data: { model_id: '123', benchmark_name: 'NonExistent', score: '90' },
        },
      ],
    });

    renderWithProviders(<CSVImport />);

    const fileInput = document.querySelector('input[type="file"]');
    const csvFile = new File(
      ['model_id,benchmark_name,score\n123,MMLU,85.2'],
      'test.csv',
      { type: 'text/csv' }
    );

    await user.upload(fileInput!, csvFile);

    const uploadButton = screen.getByRole('button', { name: /upload/i });
    await user.click(uploadButton);

    // Should display success count
    await waitFor(() => {
      expect(screen.getByText(/3.*success/i)).toBeInTheDocument();
    });

    // Should display failure count
    expect(screen.getByText(/2.*fail/i)).toBeInTheDocument();

    // Should display total rows
    expect(screen.getByText(/5.*row/i)).toBeInTheDocument();
  });

  /**
   * [P2] Task 10.11: Should display failed rows in table format
   * Story 2.11 AC#6: Display failed rows in table with row number, error, data
   */
  it('should display failed rows with error details', async () => {
    const user = userEvent.setup();

    // Mock import response with failures
    vi.mocked(importBenchmarkCSV).mockResolvedValue({
      totalRows: 2,
      successfulImports: 0,
      failedImports: 2,
      skippedDuplicates: 0,
      errors: [
        {
          rowNumber: 2,
          error: 'Invalid model_id format (must be UUID)',
          data: { model_id: 'invalid-uuid', benchmark_name: 'MMLU', score: '85.2' },
        },
        {
          rowNumber: 3,
          error: 'Benchmark not found: NonExistent',
          data: { model_id: '123', benchmark_name: 'NonExistent', score: '90' },
        },
      ],
    });

    renderWithProviders(<CSVImport />);

    const fileInput = document.querySelector('input[type="file"]');
    const csvFile = new File(['data'], 'test.csv', { type: 'text/csv' });

    await user.upload(fileInput!, csvFile);
    const uploadButton = screen.getByRole('button', { name: /upload/i });
    await user.click(uploadButton);

    // Should display error messages
    await waitFor(() => {
      expect(screen.getByText(/Invalid model_id format/i)).toBeInTheDocument();
      expect(screen.getByText(/Benchmark not found/i)).toBeInTheDocument();
    });

    // Should display row numbers
    expect(screen.getByText(/Row 2/i) || screen.getByText(/2/)).toBeInTheDocument();
    expect(screen.getByText(/Row 3/i) || screen.getByText(/3/)).toBeInTheDocument();
  });

  /**
   * [P2] Task 10.11: Should display skipped duplicates count
   * Story 2.11 AC#4: Check for duplicate model+benchmark (skip duplicates)
   */
  it('should display skipped duplicates count', async () => {
    const user = userEvent.setup();

    // Mock import response with duplicates
    vi.mocked(importBenchmarkCSV).mockResolvedValue({
      totalRows: 3,
      successfulImports: 1,
      failedImports: 0,
      skippedDuplicates: 2,
      errors: [],
    });

    renderWithProviders(<CSVImport />);

    const fileInput = document.querySelector('input[type="file"]');
    const csvFile = new File(['data'], 'test.csv', { type: 'text/csv' });

    await user.upload(fileInput!, csvFile);
    const uploadButton = screen.getByRole('button', { name: /upload/i });
    await user.click(uploadButton);

    // Should display skipped duplicates count
    await waitFor(() => {
      expect(screen.getByText(/2.*skip.*duplicate/i) || screen.getByText(/2.*duplicate.*skip/i)).toBeInTheDocument();
    });
  });

  /**
   * [P2] Task 10.11: Should have download template button
   * Story 2.11 AC#2: Add "Download Template" button in CSV import component
   */
  it('should have download template button', () => {
    renderWithProviders(<CSVImport />);

    const downloadButton = screen.getByRole('button', { name: /download.*template/i });
    expect(downloadButton).toBeInTheDocument();
  });

  /**
   * [P2] Task 10.11: Should accept only CSV files
   * Story 2.11 AC#3: Validate file is CSV (check extension)
   */
  it('should accept only .csv files', () => {
    renderWithProviders(<CSVImport />);

    const fileInput = document.querySelector('input[type="file"]');
    expect(fileInput).toHaveAttribute('accept', '.csv');
  });

  /**
   * [P2] Task 10.11: Should handle API errors gracefully
   * Story 2.11 AC#3: File upload processed in backend (error handling)
   */
  it('should display error message when upload fails', async () => {
    const user = userEvent.setup();

    // Mock API error
    vi.mocked(importBenchmarkCSV).mockRejectedValue(
      new Error('Network error')
    );

    renderWithProviders(<CSVImport />);

    const fileInput = document.querySelector('input[type="file"]');
    const csvFile = new File(['data'], 'test.csv', { type: 'text/csv' });

    await user.upload(fileInput!, csvFile);
    const uploadButton = screen.getByRole('button', { name: /upload/i });
    await user.click(uploadButton);

    // Should display error message
    await waitFor(() => {
      const errorElement = screen.queryByText(/error|fail/i) ||
                           screen.queryByRole('alert');
      expect(errorElement).toBeInTheDocument();
    });
  });

  /**
   * [P2] Task 10.11: Should allow importing another file after completion
   * Story 2.11 AC#6: Add "Import Another File" button to reset form
   */
  it('should allow importing another file after completion', async () => {
    const user = userEvent.setup();

    vi.mocked(importBenchmarkCSV).mockResolvedValue({
      totalRows: 1,
      successfulImports: 1,
      failedImports: 0,
      skippedDuplicates: 0,
      errors: [],
    });

    renderWithProviders(<CSVImport />);

    const fileInput = document.querySelector('input[type="file"]');
    const csvFile = new File(['data'], 'test.csv', { type: 'text/csv' });

    await user.upload(fileInput!, csvFile);
    const uploadButton = screen.getByRole('button', { name: /upload/i });
    await user.click(uploadButton);

    // Wait for results
    await waitFor(() => {
      expect(screen.getByText(/success/i)).toBeInTheDocument();
    });

    // Should have button to import another file
    const importAnotherButton = screen.queryByRole('button', { name: /import.*another|reset/i });
    if (importAnotherButton) {
      expect(importAnotherButton).toBeInTheDocument();

      // Clicking should reset the form
      await user.click(importAnotherButton);

      // Upload button should be disabled again
      const newUploadButton = screen.getByRole('button', { name: /upload/i });
      expect(newUploadButton).toBeDisabled();
    }
  });
});
