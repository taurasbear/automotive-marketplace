import { render, screen, fireEvent, waitFor, act } from "@testing-library/react";
import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { CompareSearchModal } from "./CompareSearchModal";
import type { SearchListingsResponse } from "../types/SearchListingsResponse";
import type { SavedListing } from "@/features/savedListings/types/SavedListing";

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
  vi.useFakeTimers({ shouldAdvanceTime: true });
  vi.clearAllMocks();
  useAppSelectorMock.mockReturnValue(null);
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

afterEach(() => {
  vi.useRealTimers();
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

    fireEvent.change(screen.getByRole("textbox"), { target: { value: "camry" } });
    await act(async () => { vi.advanceTimersByTime(300); });

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

    fireEvent.change(screen.getByRole("textbox"), { target: { value: "camry" } });
    await act(async () => { vi.advanceTimersByTime(300); });

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

    fireEvent.change(screen.getByRole("textbox"), { target: { value: "camry" } });
    await act(async () => { vi.advanceTimersByTime(300); });

    await waitFor(() =>
      expect(screen.getAllByRole("button", { name: "Compare" })).toHaveLength(
        searchResults.length,
      ),
    );

    fireEvent.click(screen.getAllByRole("button", { name: "Compare" })[0]);
    expect(onSelect).toHaveBeenCalledWith("listing-1");
    expect(onClose).not.toHaveBeenCalled();
  });
});

const savedListings: SavedListing[] = [
  {
    listingId: "saved-111",
    title: "2021 BMW 3 Series",
    price: 25000,
    city: "Klaipeda",
    mileage: 30000,
    fuelName: "Diesel",
    transmissionName: "Automatic",
    thumbnail: { url: "https://example.com/bmw.jpg", altText: "BMW" },
    noteContent: null,
  },
  {
    listingId: "saved-222",
    title: "2018 Audi A4",
    price: 18000,
    city: "Vilnius",
    mileage: 60000,
    fuelName: "Petrol",
    transmissionName: "Automatic",
    thumbnail: null,
    noteContent: null,
  },
];

describe("CompareSearchModal — liked listings (empty query)", () => {
  beforeEach(() => {
    useAppSelectorMock.mockReturnValue("user-1");
    getSavedListingsOptionsMock.mockReturnValue({
      queryKey: ["saved-listings"],
      queryFn: async () => ({ data: savedListings }),
    });
    searchListingsOptionsMock.mockReturnValue({
      queryKey: ["search", ""],
      queryFn: async () => ({ data: [] }),
      enabled: false,
    });
  });

  it("shows Your saved listings section with saved items when query is empty and user is logged in", async () => {
    render(
      <CompareSearchModal
        open={true}
        onClose={vi.fn()}
        excludeIds={[]}
        onSelect={vi.fn()}
      />,
      { wrapper: createWrapper() },
    );

    await waitFor(() =>
      expect(screen.getByText("Your saved listings")).toBeInTheDocument(),
    );
    expect(screen.getByText("2021 BMW 3 Series")).toBeInTheDocument();
    expect(screen.getByText("2018 Audi A4")).toBeInTheDocument();
  });

  it("calls onSelect with the saved listing's listingId when Compare is clicked", async () => {
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
      expect(screen.getByText("2021 BMW 3 Series")).toBeInTheDocument(),
    );

    fireEvent.click(screen.getAllByRole("button", { name: "Compare" })[0]);
    expect(onSelect).toHaveBeenCalledWith("saved-111");
  });

  it("filters saved listings by excludeIds", async () => {
    render(
      <CompareSearchModal
        open={true}
        onClose={vi.fn()}
        excludeIds={["saved-111"]}
        onSelect={vi.fn()}
      />,
      { wrapper: createWrapper() },
    );

    await waitFor(() =>
      expect(screen.getByText("Your saved listings")).toBeInTheDocument(),
    );
    expect(screen.queryByText("2021 BMW 3 Series")).not.toBeInTheDocument();
    expect(screen.getByText("2018 Audi A4")).toBeInTheDocument();
  });

  it("shows nothing when user is not logged in", async () => {
    useAppSelectorMock.mockReturnValue(null);
    render(
      <CompareSearchModal
        open={true}
        onClose={vi.fn()}
        excludeIds={[]}
        onSelect={vi.fn()}
      />,
      { wrapper: createWrapper() },
    );

    // Wait for the modal to settle (search input is always rendered)
    await waitFor(() =>
      expect(screen.getByRole("textbox")).toBeInTheDocument(),
    );
    expect(screen.queryByText("Your saved listings")).not.toBeInTheDocument();
  });

  it("hides the saved listings section when all items are excluded", async () => {
    render(
      <CompareSearchModal
        open={true}
        onClose={vi.fn()}
        excludeIds={["saved-111", "saved-222"]}
        onSelect={vi.fn()}
      />,
      { wrapper: createWrapper() },
    );

    await waitFor(() =>
      expect(screen.getByRole("textbox")).toBeInTheDocument(),
    );
    expect(screen.queryByText("Your saved listings")).not.toBeInTheDocument();
  });
});

const searchResultsWithSaved: SearchListingsResponse[] = [
  {
    id: "saved-111",
    makeName: "BMW",
    modelName: "3 Series",
    year: 2021,
    price: 25000,
    mileage: 30000,
    city: "Klaipeda",
    sellerName: "Max",
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

describe("CompareSearchModal — liked listings (with query)", () => {
  beforeEach(() => {
    useAppSelectorMock.mockReturnValue("user-1");
    getSavedListingsOptionsMock.mockReturnValue({
      queryKey: ["saved-listings"],
      queryFn: async () => ({ data: savedListings }),
    });
    searchListingsOptionsMock.mockReturnValue({
      queryKey: ["search", "bmw"],
      queryFn: async () => ({ data: searchResultsWithSaved }),
      enabled: true,
    });
  });

  it("promotes saved matches to the top with a ❤ Saved badge", async () => {
    render(
      <CompareSearchModal
        open={true}
        onClose={vi.fn()}
        excludeIds={[]}
        onSelect={vi.fn()}
      />,
      { wrapper: createWrapper() },
    );

    fireEvent.change(screen.getByRole("textbox"), { target: { value: "bmw" } });
    await act(async () => { vi.advanceTimersByTime(300); });

    await waitFor(() =>
      expect(screen.getByText(/❤ Saved/)).toBeInTheDocument(),
    );

    const compareButtons = screen.getAllByRole("button", { name: "Compare" });
    expect(compareButtons).toHaveLength(2);

    // BMW (saved match) appears before Honda (unsaved)
    const allText = document.body.textContent ?? "";
    expect(allText.indexOf("BMW")).toBeLessThan(allText.indexOf("Honda"));
  });

  it("filters search results by excludeIds", async () => {
    render(
      <CompareSearchModal
        open={true}
        onClose={vi.fn()}
        excludeIds={["saved-111"]}
        onSelect={vi.fn()}
      />,
      { wrapper: createWrapper() },
    );

    fireEvent.change(screen.getByRole("textbox"), { target: { value: "bmw" } });
    await act(async () => { vi.advanceTimersByTime(300); });

    await waitFor(() =>
      expect(screen.getByText(/Honda/)).toBeInTheDocument(),
    );
    expect(screen.queryByText(/BMW/)).not.toBeInTheDocument();
  });
});
