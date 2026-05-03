import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { describe, it, expect, vi, beforeEach } from "vitest";
import Settings from "./Settings";

const { mockUseQuery, mockMutateAsync } = vi.hoisted(() => ({
  mockUseQuery: vi.fn(),
  mockMutateAsync: vi.fn(),
}));

vi.mock("@tanstack/react-query", async () => {
  const actual = await import("@tanstack/react-query");
  return {
    ...actual,
    useQuery: (...args: unknown[]) => mockUseQuery(...args),
  };
});

vi.mock("react-i18next", () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}));

vi.mock("@/features/userPreferences", () => ({
  getUserPreferencesOptions: { queryKey: ["user-preferences"] },
  useUpsertUserPreferences: () => ({ mutateAsync: mockMutateAsync }),
  UserPreferencesDialog: ({
    open,
  }: {
    open: boolean;
    onOpenChange: (v: boolean) => void;
  }) => (open ? <div data-testid="preferences-dialog">Dialog</div> : null),
}));

const defaultPrefs = {
  valueWeight: 0.26,
  efficiencyWeight: 0.21,
  reliabilityWeight: 0.21,
  mileageWeight: 0.17,
  conditionWeight: 0.15,
  autoGenerateAiSummary: true,
  enableVehicleScoring: true,
  hasCompletedQuiz: false,
  enableMarketPriceApi: false,
};

const createWrapper = () => {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  });
  return ({ children }: { children: React.ReactNode }) => (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  );
};

describe("Settings", () => {
  beforeEach(() => {
    mockMutateAsync.mockReset();
    mockMutateAsync.mockResolvedValue({});
    mockUseQuery.mockReturnValue({ data: { data: defaultPrefs } });
  });

  it("renders the settings title and preference switches", () => {
    render(<Settings />, { wrapper: createWrapper() });
    expect(screen.getByText("settings.title")).toBeInTheDocument();
    expect(screen.getByText("settings.scoringLabel")).toBeInTheDocument();
    expect(screen.getByText("settings.aiSummaryLabel")).toBeInTheDocument();
    expect(screen.getByText("settings.marketPriceLabel")).toBeInTheDocument();
  });

  it("renders reset defaults button", () => {
    render(<Settings />, { wrapper: createWrapper() });
    expect(
      screen.getByRole("button", { name: "settings.resetButton" }),
    ).toBeInTheDocument();
  });

  it("calls upsert mutation when scoring toggle is changed", async () => {
    const user = userEvent.setup();
    render(<Settings />, { wrapper: createWrapper() });
    const switches = screen.getAllByRole("switch");
    // First switch is scoring
    await user.click(switches[0]);
    await waitFor(() => {
      expect(mockMutateAsync).toHaveBeenCalledWith(
        expect.objectContaining({ enableVehicleScoring: false }),
      );
    });
  });

  it("calls upsert mutation when AI summary toggle is changed", async () => {
    const user = userEvent.setup();
    render(<Settings />, { wrapper: createWrapper() });
    const switches = screen.getAllByRole("switch");
    // Second switch is AI summary
    await user.click(switches[1]);
    await waitFor(() => {
      expect(mockMutateAsync).toHaveBeenCalledWith(
        expect.objectContaining({ autoGenerateAiSummary: false }),
      );
    });
  });

  it("calls upsert mutation when market price toggle is changed", async () => {
    const user = userEvent.setup();
    mockUseQuery.mockReturnValue({
      data: { data: { ...defaultPrefs, enableMarketPriceApi: false } },
    });
    render(<Settings />, { wrapper: createWrapper() });
    const switches = screen.getAllByRole("switch");
    // Third switch is market price
    await user.click(switches[2]);
    await waitFor(() => {
      expect(mockMutateAsync).toHaveBeenCalledWith(
        expect.objectContaining({ enableMarketPriceApi: true }),
      );
    });
  });

  it("calls upsert with default weights when reset button is clicked", async () => {
    const user = userEvent.setup();
    render(<Settings />, { wrapper: createWrapper() });
    await user.click(
      screen.getByRole("button", { name: "settings.resetButton" }),
    );
    await waitFor(() => {
      expect(mockMutateAsync).toHaveBeenCalledWith(
        expect.objectContaining({
          valueWeight: 0.26,
          efficiencyWeight: 0.21,
          reliabilityWeight: 0.21,
          mileageWeight: 0.17,
          conditionWeight: 0.15,
        }),
      );
    });
  });
});
