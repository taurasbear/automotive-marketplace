import { render, screen } from "@testing-library/react";
import { describe, it, expect, vi, beforeEach } from "vitest";
import MainPage from "./MainPage";

const { mockUseAppSelector } = vi.hoisted(() => ({
  mockUseAppSelector: vi.fn(),
}));

vi.mock("@/hooks/redux", () => ({
  useAppSelector: (...args: unknown[]) => mockUseAppSelector(...args),
  useAppDispatch: () => vi.fn(),
}));

vi.mock("@/features/search", () => ({
  ListingSearch: ({ className }: { className?: string }) => (
    <div data-testid="listing-search" className={className}>
      ListingSearch
    </div>
  ),
}));

vi.mock("@/features/dashboard", () => ({
  Dashboard: () => <div data-testid="dashboard">Dashboard</div>,
}));

describe("MainPage", () => {
  beforeEach(() => {
    mockUseAppSelector.mockReset();
  });

  it("always renders ListingSearch", () => {
    mockUseAppSelector.mockImplementation((selector: (state: { auth: { userId: string | null } }) => unknown) =>
      selector({ auth: { userId: null } }),
    );
    render(<MainPage />);
    expect(screen.getByTestId("listing-search")).toBeInTheDocument();
  });

  it("shows Dashboard when user is authenticated", () => {
    mockUseAppSelector.mockImplementation((selector: (state: { auth: { userId: string | null } }) => unknown) =>
      selector({ auth: { userId: "user-123" } }),
    );
    render(<MainPage />);
    expect(screen.getByTestId("dashboard")).toBeInTheDocument();
  });

  it("hides Dashboard when user is a guest (userId is null)", () => {
    mockUseAppSelector.mockImplementation((selector: (state: { auth: { userId: string | null } }) => unknown) =>
      selector({ auth: { userId: null } }),
    );
    render(<MainPage />);
    expect(screen.queryByTestId("dashboard")).not.toBeInTheDocument();
  });
});
