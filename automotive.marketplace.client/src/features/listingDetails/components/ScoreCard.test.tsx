import { render, screen, fireEvent } from "@testing-library/react";
import { describe, it, expect, vi, beforeEach } from "vitest";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { ScoreCard } from "./ScoreCard";

const { mockUseAppSelector } = vi.hoisted(() => ({
  mockUseAppSelector: vi.fn(),
}));

vi.mock("@/hooks/redux", () => ({
  useAppSelector: (...args: unknown[]) => mockUseAppSelector(...args),
}));

vi.mock("react-i18next", () => ({
  useTranslation: () => ({
    t: (key: string, opts?: Record<string, unknown>) =>
      opts ? `${key}:${JSON.stringify(opts)}` : key,
  }),
}));

vi.mock("@/features/userPreferences", () => ({
  getUserPreferencesOptions: {
    queryKey: ["userPreferences"],
    queryFn: () =>
      Promise.resolve({
        data: { enableVehicleScoring: true, autoGenerateAiSummary: true },
      }),
  },
  UserPreferencesDialog: ({
    open,
  }: {
    open: boolean;
    onOpenChange: (v: boolean) => void;
  }) => (open ? <div data-testid="prefs-dialog">Prefs</div> : null),
}));

const mockScoreData = {
  data: {
    overallScore: 75,
    value: { score: 80, status: "scored" as const, weight: 1 },
    efficiency: { score: 70, status: "scored" as const, weight: 1 },
    reliability: { score: 60, status: "missing" as const, weight: 1 },
    mileage: { score: 85, status: "scored" as const, weight: 1 },
    condition: { score: 90, status: "scored" as const, weight: 1 },
    hasMissingFactors: true,
    missingFactors: ["Reliability"],
    isPersonalized: true,
  },
};

vi.mock("../api/getListingScoreOptions", () => ({
  getListingScoreOptions: () => ({
    queryKey: ["listing", "score", "listing-1"],
    queryFn: () => Promise.resolve(mockScoreData),
  }),
}));

const createWrapper = () => {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  });
  queryClient.setQueryData(["listing", "score", "listing-1"], mockScoreData);
  queryClient.setQueryData(["userPreferences"], {
    data: { enableVehicleScoring: true, autoGenerateAiSummary: true },
  });
  return ({ children }: { children: React.ReactNode }) => (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  );
};

describe("ScoreCard", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockUseAppSelector.mockImplementation((selector: (state: unknown) => unknown) =>
      selector({ auth: { userId: "user-1", permissions: [] } }),
    );
  });

  it("renders scoring disabled message when scoring is off", () => {
    const queryClient = new QueryClient({
      defaultOptions: { queries: { retry: false } },
    });
    queryClient.setQueryData(["userPreferences"], {
      data: { enableVehicleScoring: false },
    });
    render(
      <QueryClientProvider client={queryClient}>
        <ScoreCard listingId="listing-1" />
      </QueryClientProvider>,
    );
    expect(screen.getByText("score.enableScoring")).toBeInTheDocument();
  });

  it("renders overall score value", () => {
    render(<ScoreCard listingId="listing-1" />, { wrapper: createWrapper() });
    expect(screen.getByText("75")).toBeInTheDocument();
  });

  it("renders personalized label", () => {
    render(<ScoreCard listingId="listing-1" />, { wrapper: createWrapper() });
    expect(screen.getByText("score.personalized")).toBeInTheDocument();
  });

  it("shows missing factors warning when collapsed", () => {
    render(<ScoreCard listingId="listing-1" />, { wrapper: createWrapper() });
    expect(
      screen.getByText(
        /score.missingFactors/,
      ),
    ).toBeInTheDocument();
  });

  it("expands breakdown when expand button is clicked", () => {
    render(<ScoreCard listingId="listing-1" />, { wrapper: createWrapper() });
    const expandBtn = screen.getByLabelText("Expand score breakdown");
    fireEvent.click(expandBtn);
    expect(screen.getByLabelText("Collapse score breakdown")).toBeInTheDocument();
    expect(screen.getByText("score.value")).toBeInTheDocument();
    expect(screen.getByText("score.efficiency")).toBeInTheDocument();
  });

  it("shows missing status for reliability factor when expanded", () => {
    render(<ScoreCard listingId="listing-1" />, { wrapper: createWrapper() });
    fireEvent.click(screen.getByLabelText("Expand score breakdown"));
    expect(screen.getByText("score.noData")).toBeInTheDocument();
  });

  it("shows personalize button for authenticated users", () => {
    render(<ScoreCard listingId="listing-1" />, { wrapper: createWrapper() });
    expect(
      screen.getByLabelText("Personalize score weights"),
    ).toBeInTheDocument();
  });

  it("does not show personalize button for unauthenticated users", () => {
    mockUseAppSelector.mockImplementation((selector: (state: unknown) => unknown) =>
      selector({ auth: { userId: null, permissions: [] } }),
    );
    render(<ScoreCard listingId="listing-1" />, { wrapper: createWrapper() });
    expect(
      screen.queryByLabelText("Personalize score weights"),
    ).not.toBeInTheDocument();
  });
});
