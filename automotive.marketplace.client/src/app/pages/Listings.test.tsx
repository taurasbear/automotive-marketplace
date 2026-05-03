import { render, screen } from "@testing-library/react";
import { describe, it, expect, vi, beforeEach } from "vitest";
import Listings from "./Listings";

const { mockUseSearch, mockNavigate } = vi.hoisted(() => ({
  mockUseSearch: vi.fn(),
  mockNavigate: vi.fn(),
}));

vi.mock("@/app/routes/listings", () => ({
  Route: {
    useSearch: () => mockUseSearch(),
    useNavigate: () => mockNavigate,
  },
}));

vi.mock("@/features/listingList", () => ({
  Filters: ({
    searchParams,
    onSearchParamChange,
  }: {
    searchParams: Record<string, unknown>;
    onSearchParamChange: (p: Record<string, unknown>) => void;
  }) => (
    <div data-testid="filters">
      <span data-testid="filter-make">{String(searchParams.makeId ?? "")}</span>
      <button onClick={() => onSearchParamChange({ makeId: "new-make" })}>
        Change Filter
      </button>
    </div>
  ),
  ListingList: ({
    page,
    onPageChange,
  }: {
    page: number;
    onPageChange: (p: number) => void;
  }) => (
    <div data-testid="listing-list">
      <span data-testid="current-page">{page}</span>
      <button onClick={() => onPageChange(2)}>Go Page 2</button>
    </div>
  ),
}));

vi.mock("@/components/ui/skeleton", () => ({
  Skeleton: ({ className }: { className?: string }) => (
    <div data-testid="skeleton" className={className} />
  ),
}));

describe("Listings page", () => {
  beforeEach(() => {
    mockUseSearch.mockReturnValue({ page: 1 });
    mockNavigate.mockReset();
  });

  it("renders Filters and ListingList components", () => {
    render(<Listings />);

    expect(screen.getByTestId("filters")).toBeInTheDocument();
    expect(screen.getByTestId("listing-list")).toBeInTheDocument();
  });

  it("passes searchParams to Filters", () => {
    mockUseSearch.mockReturnValue({ page: 1, makeId: "make-abc" });
    render(<Listings />);

    expect(screen.getByTestId("filter-make")).toHaveTextContent("make-abc");
  });

  it("passes current page to ListingList", () => {
    mockUseSearch.mockReturnValue({ page: 3 });
    render(<Listings />);

    expect(screen.getByTestId("current-page")).toHaveTextContent("3");
  });

  it("defaults to page 1 when page is not in search params", () => {
    mockUseSearch.mockReturnValue({});
    render(<Listings />);

    expect(screen.getByTestId("current-page")).toHaveTextContent("1");
  });

  it("navigates with page 1 when filter changes", async () => {
    const { default: userEvent } = await import("@testing-library/user-event");
    const user = userEvent.setup();
    mockUseSearch.mockReturnValue({ page: 3 });
    render(<Listings />);

    await user.click(screen.getByText("Change Filter"));

    expect(mockNavigate).toHaveBeenCalledWith({
      search: { makeId: "new-make", page: 1 },
    });
  });

  it("navigates with new page number when pagination changes", async () => {
    const { default: userEvent } = await import("@testing-library/user-event");
    const user = userEvent.setup();
    mockUseSearch.mockReturnValue({ page: 1, makeId: "make-x" });
    render(<Listings />);

    await user.click(screen.getByText("Go Page 2"));

    expect(mockNavigate).toHaveBeenCalledWith({
      search: { makeId: "make-x", page: 2 },
    });
  });
});
