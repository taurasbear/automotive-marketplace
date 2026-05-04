import { render, screen } from "@testing-library/react";
import { describe, it, expect, vi, beforeEach } from "vitest";
import { Dashboard } from "./Dashboard";
import type { GetDashboardSummaryResponse } from "../types/GetDashboardSummaryResponse";

const { mockUseQuery, mockUseRouter } = vi.hoisted(() => ({
  mockUseQuery: vi.fn(),
  mockUseRouter: vi.fn(),
}));

vi.mock("@tanstack/react-query", () => ({
  useQuery: (...args: unknown[]) => mockUseQuery(...args),
  queryOptions: (opts: unknown) => opts,
  QueryClient: vi.fn(),
  QueryClientProvider: ({ children }: { children: React.ReactNode }) => <>{children}</>,
}));

vi.mock("@tanstack/react-router", () => ({
  useRouter: () => mockUseRouter(),
  createRootRouteWithContext: () => () => ({}),
  createRoute: () => ({}),
  createFileRoute: () => () => ({}),
}));

vi.mock("react-i18next", () => ({
  useTranslation: () => ({ t: (key: string) => key }),
  initReactI18next: { type: "3rdParty", init: () => {} },
}));

vi.mock("@/lib/i18n/formatNumber", () => ({
  formatCurrency: (val: number) => `€${val}`,
}));

vi.mock("../api/getDashboardSummaryOptions", () => ({
  getDashboardSummaryOptions: { queryKey: ["dashboard-summary"] },
}));

const mockDashboardData: GetDashboardSummaryResponse = {
  offers: {
    pendingCount: 2,
    newestOfferListing: "BMW 320d",
    newestOfferAmount: 15000,
    newestOfferFrom: "John",
  },
  meetings: {
    upcomingCount: 1,
    nextMeetingAt: "2024-12-01T10:00:00Z",
    nextMeetingCounterpart: "Jane",
    nextMeetingListing: "Audi A4",
  },
  contracts: {
    actionNeededCount: 0,
    nextActionListing: null,
    nextActionType: null,
  },
  availability: {
    pendingCount: 3,
  },
};

describe("Dashboard", () => {
  beforeEach(() => {
    mockUseRouter.mockReturnValue({ navigate: vi.fn() });
  });

  it("renders loading state when data is loading", () => {
    mockUseQuery.mockReturnValue({ data: undefined, isLoading: true });
    render(<Dashboard />);
    const pulseDivs = document.querySelectorAll(".animate-pulse");
    expect(pulseDivs.length).toBeGreaterThan(0);
  });

  it("renders nothing when data is undefined and not loading", () => {
    mockUseQuery.mockReturnValue({ data: undefined, isLoading: false });
    const { container } = render(<Dashboard />);
    expect(container.innerHTML).toBe("");
  });

  it("renders all four dashboard tiles with data", () => {
    mockUseQuery.mockReturnValue({
      data: mockDashboardData,
      isLoading: false,
    });
    render(<Dashboard />);
    expect(screen.getByText("offers.title")).toBeInTheDocument();
    expect(screen.getByText("meetings.title")).toBeInTheDocument();
    expect(screen.getByText("contracts.title")).toBeInTheDocument();
    expect(screen.getByText("availability.title")).toBeInTheDocument();
  });

  it("displays offer detail with formatted currency", () => {
    mockUseQuery.mockReturnValue({
      data: mockDashboardData,
      isLoading: false,
    });
    render(<Dashboard />);
    expect(screen.getByText("€15000 — John")).toBeInTheDocument();
  });

  it("navigates to inbox when a tile is clicked", async () => {
    const mockNavigate = vi.fn();
    mockUseRouter.mockReturnValue({ navigate: mockNavigate });
    mockUseQuery.mockReturnValue({
      data: mockDashboardData,
      isLoading: false,
    });
    const user = (await import("@testing-library/user-event")).default.setup();
    render(<Dashboard />);
    await user.click(screen.getByText("offers.title"));
    expect(mockNavigate).toHaveBeenCalledWith({ to: "/inbox" });
  });
});
