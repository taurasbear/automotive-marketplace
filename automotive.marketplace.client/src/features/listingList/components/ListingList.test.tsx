import { render, screen } from "@testing-library/react";
import { describe, it, expect, vi } from "vitest";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { Suspense } from "react";
import ListingList from "./ListingList";

const { mockGetAllListingsOptions } = vi.hoisted(() => ({
  mockGetAllListingsOptions: vi.fn(),
}));

vi.mock("../api/getAllListingsOptions", () => ({
  getAllListingsOptions: (...args: unknown[]) =>
    mockGetAllListingsOptions(...args),
}));

vi.mock("./ListingCard", () => ({
  default: ({ listing }: { listing: { id: string; makeName: string } }) => (
    <div data-testid={`listing-card-${listing.id}`}>{listing.makeName}</div>
  ),
}));

vi.mock("@/components/ui/Pagination", () => ({
  Pagination: ({
    page,
    totalPages,
    onPageChange,
  }: {
    page: number;
    totalPages: number;
    onPageChange: (p: number) => void;
  }) => (
    <div data-testid="pagination">
      <span data-testid="page-info">
        {page}/{totalPages}
      </span>
      <button onClick={() => onPageChange(page + 1)}>Next</button>
    </div>
  ),
}));

const mockListings = [
  { id: "l1", makeName: "BMW" },
  { id: "l2", makeName: "Audi" },
  { id: "l3", makeName: "Mercedes" },
];

function createQueryClient(items = mockListings, totalPages = 3) {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  });

  mockGetAllListingsOptions.mockReturnValue({
    queryKey: ["listings", "all"],
    queryFn: () =>
      Promise.resolve({
        data: { items, total: items.length, page: 1, pageSize: 20, totalPages },
      }),
  });

  // Pre-populate cache for useSuspenseQuery
  queryClient.setQueryData(["listings", "all"], {
    data: { items, total: items.length, page: 1, pageSize: 20, totalPages },
  });

  return queryClient;
}

function renderWithProviders(
  ui: React.ReactElement,
  queryClient: QueryClient,
) {
  return render(
    <QueryClientProvider client={queryClient}>
      <Suspense fallback={<div>Loading...</div>}>{ui}</Suspense>
    </QueryClientProvider>,
  );
}

describe("ListingList", () => {
  it("renders a ListingCard for each listing", () => {
    const queryClient = createQueryClient();

    renderWithProviders(
      <ListingList
        listingSearchQuery={{}}
        page={1}
        onPageChange={vi.fn()}
      />,
      queryClient,
    );

    expect(screen.getByTestId("listing-card-l1")).toBeInTheDocument();
    expect(screen.getByTestId("listing-card-l2")).toBeInTheDocument();
    expect(screen.getByTestId("listing-card-l3")).toBeInTheDocument();
  });

  it("renders make names in listing cards", () => {
    const queryClient = createQueryClient();

    renderWithProviders(
      <ListingList
        listingSearchQuery={{}}
        page={1}
        onPageChange={vi.fn()}
      />,
      queryClient,
    );

    expect(screen.getByText("BMW")).toBeInTheDocument();
    expect(screen.getByText("Audi")).toBeInTheDocument();
    expect(screen.getByText("Mercedes")).toBeInTheDocument();
  });

  it("renders pagination with correct page info", () => {
    const queryClient = createQueryClient(mockListings, 5);

    renderWithProviders(
      <ListingList
        listingSearchQuery={{}}
        page={2}
        onPageChange={vi.fn()}
      />,
      queryClient,
    );

    expect(screen.getByTestId("page-info")).toHaveTextContent("2/5");
  });

  it("calls onPageChange when pagination is clicked", async () => {
    const { default: userEvent } = await import("@testing-library/user-event");
    const user = userEvent.setup();
    const mockOnPageChange = vi.fn();
    const queryClient = createQueryClient(mockListings, 5);

    renderWithProviders(
      <ListingList
        listingSearchQuery={{}}
        page={2}
        onPageChange={mockOnPageChange}
      />,
      queryClient,
    );

    await user.click(screen.getByText("Next"));
    expect(mockOnPageChange).toHaveBeenCalledWith(3);
  });

  it("renders empty list when no listings", () => {
    const queryClient = createQueryClient([], 0);

    renderWithProviders(
      <ListingList
        listingSearchQuery={{}}
        page={1}
        onPageChange={vi.fn()}
      />,
      queryClient,
    );

    expect(screen.queryByTestId(/listing-card/)).not.toBeInTheDocument();
  });
});
