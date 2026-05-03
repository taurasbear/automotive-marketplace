import { render, screen } from "@testing-library/react";
import { describe, it, expect, vi, beforeEach } from "vitest";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import SavedListingsPage from "./SavedListingsPage";
import type { SavedListing } from "../types/SavedListing";

const { mockUseAppSelector } = vi.hoisted(() => ({
  mockUseAppSelector: vi.fn(),
}));

vi.mock("@/hooks/redux", () => ({
  useAppSelector: (...args: unknown[]) => mockUseAppSelector(...args),
}));

vi.mock("@/features/auth", () => ({
  selectAccessToken: "selectAccessToken",
}));

vi.mock("react-i18next", () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}));

vi.mock("@tanstack/react-router", () => ({
  Link: ({ children, to }: { children: React.ReactNode; to?: string }) => (
    <a href={String(to ?? "")}>{children}</a>
  ),
}));

vi.mock("./SavedListingRow", () => ({
  default: ({ listing }: { listing: SavedListing }) => (
    <div data-testid={`listing-row-${listing.listingId}`}>{listing.title}</div>
  ),
}));

vi.mock("../api/getSavedListingsOptions", () => ({
  getSavedListingsOptions: () => ({
    queryKey: ["savedListing", "list"],
    queryFn: () => Promise.resolve({ data: [] }),
  }),
}));

const createWrapper = (listings: SavedListing[] = []) => {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  });
  queryClient.setQueryData(["savedListing", "list"], { data: listings });
  return ({ children }: { children: React.ReactNode }) => (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  );
};

const createListing = (overrides: Partial<SavedListing> = {}): SavedListing => ({
  listingId: "listing-1",
  title: "2020 BMW X5",
  price: 35000,
  municipalityName: "Vilnius",
  mileage: 80000,
  fuelName: "Diesel",
  transmissionName: "Automatic",
  thumbnail: null,
  noteContent: null,
  ...overrides,
});

describe("SavedListingsPage", () => {
  beforeEach(() => {
    mockUseAppSelector.mockReturnValue("mock-token");
  });

  it("shows empty state when no saved listings", () => {
    render(<SavedListingsPage />, { wrapper: createWrapper([]) });
    expect(screen.getByText("page.emptyState")).toBeInTheDocument();
    expect(screen.getByText("page.browseListings")).toBeInTheDocument();
  });

  it("renders browse listings link pointing to /listings", () => {
    render(<SavedListingsPage />, { wrapper: createWrapper([]) });
    const link = screen.getByText("page.browseListings").closest("a");
    expect(link).toHaveAttribute("href", "/listings");
  });

  it("renders page title and listing rows when listings exist", () => {
    const listings = [
      createListing({ listingId: "l-1", title: "Audi A4" }),
      createListing({ listingId: "l-2", title: "BMW 3 Series" }),
    ];
    render(<SavedListingsPage />, { wrapper: createWrapper(listings) });

    expect(screen.getByText("page.title")).toBeInTheDocument();
    expect(screen.getByTestId("listing-row-l-1")).toBeInTheDocument();
    expect(screen.getByTestId("listing-row-l-2")).toBeInTheDocument();
    expect(screen.getByText("Audi A4")).toBeInTheDocument();
    expect(screen.getByText("BMW 3 Series")).toBeInTheDocument();
  });

  it("renders a SavedListingRow for each listing", () => {
    const listings = [
      createListing({ listingId: "a" }),
      createListing({ listingId: "b" }),
      createListing({ listingId: "c" }),
    ];
    render(<SavedListingsPage />, { wrapper: createWrapper(listings) });

    expect(screen.getByTestId("listing-row-a")).toBeInTheDocument();
    expect(screen.getByTestId("listing-row-b")).toBeInTheDocument();
    expect(screen.getByTestId("listing-row-c")).toBeInTheDocument();
  });
});
