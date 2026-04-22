import { render, screen, waitFor, fireEvent } from "@testing-library/react";
import { describe, it, expect, vi, beforeEach } from "vitest";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import Compare from "./Compare";
import type { GetListingComparisonResponse } from "@/features/compareListings/types/GetListingComparisonResponse";
import { router } from "@/lib/router";
import type { SavedListing } from "@/features/savedListings/types/SavedListing";

// Mock the route to supply search params
vi.mock("@/app/routes/compare", () => ({
  Route: {
    useSearch: () => ({ a: "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa", b: "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb" }),
  },
}));

// Break the import chain: CompareSearchModal → router → routeTree → __root → redux store
vi.mock("@/lib/router", () => ({
  router: { navigate: vi.fn() },
}));

vi.mock("@/hooks/redux", () => ({
  useAppSelector: (...args: unknown[]) => useAppSelectorMock(...args),
}));

vi.mock("@/features/savedListings/api/getSavedListingsOptions", () => ({
  getSavedListingsOptions: getSavedListingsOptionsMock,
}));

vi.mock("@/features/compareListings/api/searchListingsOptions", () => ({
  searchListingsOptions: vi.fn().mockReturnValue({
    queryKey: ["search", ""],
    queryFn: async () => ({ data: [] }),
    enabled: false,
  }),
}));

const mockComparison: GetListingComparisonResponse = {
  listingA: {
    id: "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
    makeName: "Toyota",
    modelName: "Camry",
    price: 15000,
    powerKw: 120,
    engineSizeMl: 1998,
    mileage: 50000,
    isSteeringWheelRight: false,
    city: "Vilnius",
    isUsed: true,
    year: 2020,
    transmissionName: "Automatic",
    fuelName: "Petrol",
    doorCount: 4,
    bodyTypeName: "Sedan",
    drivetrainName: "FWD",
    sellerName: "John",
    sellerId: "s1",
    status: "Available",
    images: [],
  },
  listingB: {
    id: "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
    makeName: "Honda",
    modelName: "Civic",
    price: 12000,
    powerKw: 100,
    engineSizeMl: 1500,
    mileage: 80000,
    isSteeringWheelRight: false,
    city: "Kaunas",
    isUsed: true,
    year: 2018,
    transmissionName: "Manual",
    fuelName: "Petrol",
    doorCount: 4,
    bodyTypeName: "Sedan",
    drivetrainName: "FWD",
    sellerName: "Jane",
    sellerId: "s2",
    status: "Available",
    images: [],
  },
};

const mockSavedListing: SavedListing = {
  listingId: "cccccccc-cccc-cccc-cccc-cccccccccccc",
  title: "2022 Volkswagen Golf",
  price: 20000,
  city: "Kaunas",
  mileage: 15000,
  fuelName: "Petrol",
  transmissionName: "Automatic",
  thumbnail: null,
  noteContent: null,
};

const { getListingComparisonOptionsMock } = vi.hoisted(() => ({
  getListingComparisonOptionsMock: vi.fn(),
}));

const { useAppSelectorMock, getSavedListingsOptionsMock } = vi.hoisted(() => ({
  useAppSelectorMock: vi.fn(),
  getSavedListingsOptionsMock: vi.fn(),
}));

// Mock the API options so useQuery returns our fixture
vi.mock("@/features/compareListings/api/getListingComparisonOptions", () => ({
  getListingComparisonOptions: getListingComparisonOptionsMock,
}));

const createWrapper = () => {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  });
  return ({ children }: { children: React.ReactNode }) => (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  );
};

beforeEach(() => {
  getListingComparisonOptionsMock.mockImplementation((_a: string, _b: string) => ({
    queryKey: ["listing", "comparison", _a, _b],
    queryFn: async () => ({ data: mockComparison }),
  }));
  useAppSelectorMock.mockReturnValue(null);
  getSavedListingsOptionsMock.mockReturnValue({
    queryKey: ["saved-listings"],
    queryFn: async () => ({ data: [] }),
  });
});

describe("Compare page", () => {
  it("renders comparison table with both listing names after data loads", async () => {
    render(<Compare />, { wrapper: createWrapper() });

    await waitFor(() => {
      expect(screen.getByText("Toyota")).toBeInTheDocument();
      expect(screen.getByText("Honda")).toBeInTheDocument();
    });
  });

  it("renders all three table sections", async () => {
    render(<Compare />, { wrapper: createWrapper() });

    await waitFor(() => {
      expect(screen.getByText("Basic Info")).toBeInTheDocument();
      expect(screen.getByText("Engine & Performance")).toBeInTheDocument();
      expect(screen.getByText("Listing Details")).toBeInTheDocument();
    });
  });

  it("shows error message when query errors", async () => {
    getListingComparisonOptionsMock.mockImplementation(() => ({
      queryKey: ["listing", "comparison", "err"],
      queryFn: async () => { throw new Error("404"); },
    }));

    const queryClient = new QueryClient({
      defaultOptions: { queries: { retry: false } },
    });
    render(
      <QueryClientProvider client={queryClient}>
        <Compare />
      </QueryClientProvider>,
    );

    await waitFor(() => {
      expect(
        screen.getByText(/one or more listings could not be found/i),
      ).toBeInTheDocument();
    });
  });
});

describe("Compare page — swap orchestration", () => {
  it("opens the modal when the Change listing A button is clicked", async () => {
    render(<Compare />, { wrapper: createWrapper() });

    await waitFor(() => {
      expect(
        screen.getByRole("button", { name: "Change listing A" }),
      ).toBeInTheDocument();
    });

    expect(screen.queryByRole("dialog")).not.toBeInTheDocument();

    fireEvent.click(screen.getByRole("button", { name: "Change listing A" }));

    expect(screen.getByRole("dialog")).toBeInTheDocument();
  });

  it("opens the modal when the Change listing B button is clicked", async () => {
    render(<Compare />, { wrapper: createWrapper() });

    await waitFor(() => {
      expect(
        screen.getByRole("button", { name: "Change listing B" }),
      ).toBeInTheDocument();
    });

    expect(screen.queryByRole("dialog")).not.toBeInTheDocument();

    fireEvent.click(screen.getByRole("button", { name: "Change listing B" }));

    expect(screen.getByRole("dialog")).toBeInTheDocument();
  });

  it("closes the modal when Escape is pressed", async () => {
    render(<Compare />, { wrapper: createWrapper() });

    await waitFor(() => {
      expect(
        screen.getByRole("button", { name: "Change listing A" }),
      ).toBeInTheDocument();
    });

    fireEvent.click(screen.getByRole("button", { name: "Change listing A" }));
    expect(screen.getByRole("dialog")).toBeInTheDocument();

    fireEvent.keyDown(screen.getByRole("dialog"), { key: "Escape" });

    await waitFor(() => {
      expect(screen.queryByRole("dialog")).not.toBeInTheDocument();
    });
  });

  it("navigates with the selected listing replacing slot A when a listing is chosen", async () => {
    useAppSelectorMock.mockReturnValue("user-1");
    getSavedListingsOptionsMock.mockReturnValue({
      queryKey: ["saved-listings"],
      queryFn: async () => ({ data: [mockSavedListing] }),
    });

    render(<Compare />, { wrapper: createWrapper() });

    await waitFor(() =>
      expect(
        screen.getByRole("button", { name: "Change listing A" }),
      ).toBeInTheDocument(),
    );

    fireEvent.click(screen.getByRole("button", { name: "Change listing A" }));
    expect(screen.getByRole("dialog")).toBeInTheDocument();

    await waitFor(() =>
      expect(screen.getByText("2022 Volkswagen Golf")).toBeInTheDocument(),
    );

    fireEvent.click(screen.getByRole("button", { name: "Compare" }));

    expect(vi.mocked(router.navigate)).toHaveBeenCalledWith({
      to: "/compare",
      search: {
        a: "cccccccc-cccc-cccc-cccc-cccccccccccc",
        b: "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
      },
    });
  });

  it("navigates with the selected listing replacing slot B when a listing is chosen", async () => {
    useAppSelectorMock.mockReturnValue("user-1");
    getSavedListingsOptionsMock.mockReturnValue({
      queryKey: ["saved-listings"],
      queryFn: async () => ({ data: [mockSavedListing] }),
    });

    render(<Compare />, { wrapper: createWrapper() });

    await waitFor(() =>
      expect(
        screen.getByRole("button", { name: "Change listing B" }),
      ).toBeInTheDocument(),
    );

    fireEvent.click(screen.getByRole("button", { name: "Change listing B" }));
    expect(screen.getByRole("dialog")).toBeInTheDocument();

    await waitFor(() =>
      expect(screen.getByText("2022 Volkswagen Golf")).toBeInTheDocument(),
    );

    fireEvent.click(screen.getByRole("button", { name: "Compare" }));

    expect(vi.mocked(router.navigate)).toHaveBeenCalledWith({
      to: "/compare",
      search: {
        a: "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
        b: "cccccccc-cccc-cccc-cccc-cccccccccccc",
      },
    });
  });
});
