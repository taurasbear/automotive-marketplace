import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import { describe, it, expect, vi, beforeEach } from "vitest";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { CompareSearchModal } from "./CompareSearchModal";
import type { SearchListingsResponse } from "../types/SearchListingsResponse";

vi.mock("@/lib/router", () => ({
  router: { navigate: vi.fn() },
}));

const {
  useAppSelectorMock,
  getSavedListingsOptionsMock,
  searchListingsOptionsMock,
} = vi.hoisted(() => ({
  useAppSelectorMock: vi.fn(),
  getSavedListingsOptionsMock: vi.fn(),
  searchListingsOptionsMock: vi.fn(),
}));

// Pre-established for liked listings logic added in Task 5
vi.mock("@/hooks/redux", () => ({
  useAppSelector: (...args: unknown[]) =>
    useAppSelectorMock(...args),
}));

// Pre-established for liked listings logic added in Task 5
vi.mock("@/features/savedListings/api/getSavedListingsOptions", () => ({
  getSavedListingsOptions: getSavedListingsOptionsMock,
}));

vi.mock("../api/searchListingsOptions", () => ({
  searchListingsOptions: searchListingsOptionsMock,
}));

const searchResults: SearchListingsResponse[] = [
  {
    id: "listing-1",
    makeName: "Toyota",
    modelName: "Camry",
    year: 2020,
    price: 15000,
    mileage: 50000,
    city: "Vilnius",
    sellerName: "John",
  },
  {
    id: "listing-2",
    makeName: "Honda",
    modelName: "Civic",
    year: 2019,
    price: 12000,
    mileage: 80000,
    city: "Kaunas",
    sellerName: "Jane",
  },
];

const createWrapper = () => {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  });
  return ({ children }: { children: React.ReactNode }) => (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  );
};

beforeEach(() => {
  vi.clearAllMocks();
  useAppSelectorMock.mockReturnValue(null); // not logged in by default
  getSavedListingsOptionsMock.mockReturnValue({
    queryKey: ["saved-listings"],
    queryFn: async () => ({ data: [] }),
  });
  searchListingsOptionsMock.mockReturnValue({
    queryKey: ["search", "camry"],
    queryFn: async () => ({ data: searchResults }),
    enabled: true,
  });
});

describe("CompareSearchModal — new prop API", () => {
  it("calls onSelect with the listing id when Compare is clicked", async () => {
    const onSelect = vi.fn();
    render(
      <CompareSearchModal
        open={true}
        onClose={vi.fn()}
        excludeIds={[]}
        onSelect={onSelect}
      />,
      { wrapper: createWrapper() },
    );

    await waitFor(() =>
      expect(screen.getAllByRole("button", { name: "Compare" })).toHaveLength(
        searchResults.length,
      ),
    );

    fireEvent.click(screen.getAllByRole("button", { name: "Compare" })[0]);
    expect(onSelect).toHaveBeenCalledWith("listing-1");
    expect(onSelect).toHaveBeenCalledTimes(1);
  });

  it("filters out listings whose id is in excludeIds", async () => {
    render(
      <CompareSearchModal
        open={true}
        onClose={vi.fn()}
        excludeIds={["listing-1"]}
        onSelect={vi.fn()}
      />,
      { wrapper: createWrapper() },
    );

    await waitFor(() =>
      expect(screen.getAllByRole("button", { name: "Compare" })).toHaveLength(1),
    );

    expect(screen.queryByText(/Toyota/)).not.toBeInTheDocument();
    expect(screen.getByText(/Honda/)).toBeInTheDocument();
  });

  it("does not call onClose when a listing is selected", async () => {
    const onClose = vi.fn();
    const onSelect = vi.fn();
    render(
      <CompareSearchModal
        open={true}
        onClose={onClose}
        excludeIds={[]}
        onSelect={onSelect}
      />,
      { wrapper: createWrapper() },
    );

    await waitFor(() =>
      expect(screen.getAllByRole("button", { name: "Compare" })).toHaveLength(
        searchResults.length,
      ),
    );

    fireEvent.click(screen.getAllByRole("button", { name: "Compare" })[0]);
    expect(onSelect).toHaveBeenCalledWith("listing-1");
    expect(onClose).not.toHaveBeenCalled(); // caller is responsible for closing
  });
});
