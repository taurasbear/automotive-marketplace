import { render, screen, fireEvent } from "@testing-library/react";
import { describe, it, expect, vi, beforeEach } from "vitest";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { SellerInsightsPanel } from "./SellerInsightsPanel";

vi.mock("react-i18next", () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}));

const mockInsightsData = {
  data: {
    marketPosition: {
      listingPrice: 25000,
      marketMedianPrice: 27000,
      priceDifferencePercent: 7.4,
      hasMarketData: true,
      marketListingCount: 15,
      daysListed: 12,
    },
    listingQuality: {
      qualityScore: 85,
      hasDescription: true,
      hasPhotos: true,
      photoCount: 6,
      hasVin: true,
      hasColour: false,
      suggestions: ["addColour"],
    },
  },
};

vi.mock("@/features/listingDetails", () => ({
  getSellerListingInsightsOptions: (id: string) => ({
    queryKey: ["listing", "sellerInsights", id],
    queryFn: () => Promise.resolve(mockInsightsData),
  }),
}));

const createWrapper = (preload = true) => {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  });
  if (preload) {
    queryClient.setQueryData(
      ["listing", "sellerInsights", "listing-1"],
      mockInsightsData,
    );
  }
  return ({ children }: { children: React.ReactNode }) => (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  );
};

describe("SellerInsightsPanel", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("renders collapsed panel with title", () => {
    render(<SellerInsightsPanel listingId="listing-1" />, {
      wrapper: createWrapper(),
    });
    expect(
      screen.getByText("sellerInsights.title"),
    ).toBeInTheDocument();
  });

  it("expands to show market position and quality when clicked", () => {
    render(<SellerInsightsPanel listingId="listing-1" />, {
      wrapper: createWrapper(),
    });
    fireEvent.click(screen.getByText("sellerInsights.title"));
    expect(
      screen.getByText("sellerInsights.marketPosition.title"),
    ).toBeInTheDocument();
    expect(
      screen.getByText("sellerInsights.listingQuality.title"),
    ).toBeInTheDocument();
  });

  it("displays listing price in market position", () => {
    render(<SellerInsightsPanel listingId="listing-1" />, {
      wrapper: createWrapper(),
    });
    fireEvent.click(screen.getByText("sellerInsights.title"));
    expect(screen.getByText(/€25,000/)).toBeInTheDocument();
  });

  it("shows quality score", () => {
    render(<SellerInsightsPanel listingId="listing-1" />, {
      wrapper: createWrapper(),
    });
    fireEvent.click(screen.getByText("sellerInsights.title"));
    expect(screen.getByText("85")).toBeInTheDocument();
    expect(screen.getByText("/100")).toBeInTheDocument();
  });

  it("shows quality checklist items", () => {
    render(<SellerInsightsPanel listingId="listing-1" />, {
      wrapper: createWrapper(),
    });
    fireEvent.click(screen.getByText("sellerInsights.title"));
    expect(
      screen.getByText("sellerInsights.listingQuality.description"),
    ).toBeInTheDocument();
    expect(
      screen.getByText("sellerInsights.listingQuality.vin"),
    ).toBeInTheDocument();
  });

  it("shows suggestions when present", () => {
    render(<SellerInsightsPanel listingId="listing-1" />, {
      wrapper: createWrapper(),
    });
    fireEvent.click(screen.getByText("sellerInsights.title"));
    expect(
      screen.getByText("• sellerInsights.suggestions.addColour"),
    ).toBeInTheDocument();
  });

  it("returns null when no data and not loading", () => {
    const queryClient = new QueryClient({
      defaultOptions: { queries: { retry: false, enabled: false } },
    });
    queryClient.setQueryData(
      ["listing", "sellerInsights", "no-data"],
      undefined,
    );
    const { container } = render(
      <QueryClientProvider client={queryClient}>
        <SellerInsightsPanel listingId="no-data" />
      </QueryClientProvider>,
    );
    expect(container.firstChild).toBeNull();
  });
});
