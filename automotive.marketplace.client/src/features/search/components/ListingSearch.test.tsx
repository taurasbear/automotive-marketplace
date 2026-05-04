import { render, screen } from "@testing-library/react";
import { describe, it, expect, vi } from "vitest";
import ListingSearch from "./ListingSearch";

const { mockNavigate } = vi.hoisted(() => ({
  mockNavigate: vi.fn(),
}));

vi.mock("react-i18next", () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}));

vi.mock("@tanstack/react-router", () => ({
  useNavigate: () => mockNavigate,
  Link: ({
    children,
    to,
  }: {
    children: React.ReactNode;
    to?: string;
  }) => <a href={String(to ?? "")}>{children}</a>,
}));

vi.mock("./ListingSearchFilters", () => ({
  default: ({
    searchValues,
    updateSearchValue,
  }: {
    searchValues: Record<string, unknown>;
    updateSearchValue: (key: string, value: string) => void;
  }) => (
    <div data-testid="listing-search-filters">
      <span data-testid="make-value">{String(searchValues.makeId)}</span>
      <button onClick={() => updateSearchValue("makeId", "bmw")}>
        Change Make
      </button>
    </div>
  ),
}));

describe("ListingSearch", () => {
  it("renders search label and search button", () => {
    render(<ListingSearch />);
    expect(screen.getByText("search.lookUp")).toBeInTheDocument();
    expect(
      screen.getByRole("button", { name: "search.search" }),
    ).toBeInTheDocument();
  });

  it("renders ListingSearchFilters component", () => {
    render(<ListingSearch />);
    expect(screen.getByTestId("listing-search-filters")).toBeInTheDocument();
  });

  it("renders search button inside a link to /listings", () => {
    render(<ListingSearch />);
    const link = screen.getByRole("link");
    expect(link).toHaveAttribute("href", "/listings");
  });

  it("accepts className prop", () => {
    const { container } = render(<ListingSearch className="test-class" />);
    expect(container.firstChild).toHaveClass("test-class");
  });
});
