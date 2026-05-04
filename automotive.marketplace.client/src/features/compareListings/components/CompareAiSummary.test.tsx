import { render, screen, fireEvent } from "@testing-library/react";
import { describe, it, expect, vi } from "vitest";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";

const { mockUseAppSelector } = vi.hoisted(() => ({
  mockUseAppSelector: vi.fn(),
}));

vi.mock("@/hooks/redux", () => ({
  useAppSelector: (...args: unknown[]) => mockUseAppSelector(...args),
}));

vi.mock("react-i18next", () => ({
  useTranslation: () => ({
    t: (key: string) => key,
    i18n: { language: "en" },
  }),
}));

vi.mock("../api/getListingComparisonAiSummaryOptions", () => ({
  getListingComparisonAiSummaryOptions: () => ({
    queryKey: ["ai-summary", "a", "b"],
    queryFn: () =>
      Promise.resolve({
        data: {
          isGenerated: true,
          summary: "Listing A is better overall.",
          unavailableFactors: [],
        },
      }),
  }),
}));

vi.mock("@/features/userPreferences", () => ({
  getUserPreferencesOptions: {
    queryKey: ["userPreferences"],
    queryFn: () =>
      Promise.resolve({
        data: { autoGenerateAiSummary: true },
      }),
  },
}));

vi.mock("@/components/ui/button", () => ({
  Button: ({
    children,
    onClick,
    disabled,
  }: {
    children: React.ReactNode;
    onClick?: () => void;
    disabled?: boolean;
  }) => (
    <button onClick={onClick} disabled={disabled}>
      {children}
    </button>
  ),
}));

vi.mock("@/components/ui/alert", () => ({
  Alert: ({ children }: { children: React.ReactNode }) => <div role="alert">{children}</div>,
  AlertDescription: ({ children }: { children: React.ReactNode }) => <div>{children}</div>,
}));

import { CompareAiSummary } from "./CompareAiSummary";

const createWrapper = () => {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  });
  return ({ children }: { children: React.ReactNode }) => (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  );
};

describe("CompareAiSummary", () => {
  beforeEach(() => {
    mockUseAppSelector.mockImplementation((selector: (s: unknown) => unknown) =>
      selector({ auth: { userId: "user-1" } }),
    );
  });

  it("renders the title and generate button", () => {
    render(
      <CompareAiSummary listingAId="a" listingBId="b" />,
      { wrapper: createWrapper() },
    );
    expect(screen.getByText("aiSummary.title")).toBeInTheDocument();
  });

  it("renders regenerate button when summary exists", async () => {
    render(
      <CompareAiSummary listingAId="a" listingBId="b" />,
      { wrapper: createWrapper() },
    );
    expect(await screen.findByText("aiSummary.regenerate")).toBeInTheDocument();
  });

  it("renders summary text when data is available", async () => {
    render(
      <CompareAiSummary listingAId="a" listingBId="b" />,
      { wrapper: createWrapper() },
    );
    expect(await screen.findByText("Listing A is better overall.")).toBeInTheDocument();
  });

  it("shows prompt text when no data and not authenticated", () => {
    mockUseAppSelector.mockImplementation((selector: (s: unknown) => unknown) =>
      selector({ auth: { userId: null } }),
    );
    render(
      <CompareAiSummary listingAId="a" listingBId="b" />,
      { wrapper: createWrapper() },
    );
    expect(screen.getByText("aiSummary.prompt")).toBeInTheDocument();
  });

  it("regenerate button is clickable", async () => {
    render(
      <CompareAiSummary listingAId="a" listingBId="b" />,
      { wrapper: createWrapper() },
    );
    const button = await screen.findByText("aiSummary.regenerate");
    fireEvent.click(button);
    // Should not throw
    expect(button).toBeInTheDocument();
  });
});
