import { render, screen, waitFor } from "@testing-library/react";
import { describe, it, expect, vi, beforeEach } from "vitest";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import Compare from "./Compare";
import type { GetListingComparisonResponse } from "@/features/compareListings/types/GetListingComparisonResponse";

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

const { getListingComparisonOptionsMock } = vi.hoisted(() => ({
  getListingComparisonOptionsMock: vi.fn(),
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
