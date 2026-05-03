import { render, screen } from "@testing-library/react";
import { describe, it, expect, vi, beforeEach } from "vitest";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";

const { mockUseAppSelector } = vi.hoisted(() => ({
  mockUseAppSelector: vi.fn(),
}));

vi.mock("@/hooks/redux", () => ({
  useAppSelector: (...args: unknown[]) => mockUseAppSelector(...args),
}));

vi.mock("react-i18next", () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}));

vi.mock("@/features/userPreferences", () => ({
  UserPreferencesDialog: ({ open }: { open: boolean }) =>
    open ? <div data-testid="prefs-dialog" /> : null,
  getUserPreferencesOptions: {
    queryKey: ["userPreferences"],
    queryFn: () =>
      Promise.resolve({
        data: { enableVehicleScoring: true, hasCompletedQuiz: true },
      }),
  },
}));

vi.mock("@/features/listingDetails", () => ({
  getListingScoreOptions: (id: string) => ({
    queryKey: ["score", id],
    queryFn: () =>
      Promise.resolve({
        data: {
          overallScore: 75,
          isPersonalized: true,
          value: { score: 80, status: "ok" },
          efficiency: { score: 60, status: "ok" },
          reliability: { score: 70, status: "ok" },
          mileage: { score: 85, status: "ok" },
          condition: { score: 90, status: "ok" },
        },
      }),
  }),
}));

import { CompareScoreBanner } from "./CompareScoreBanner";

const createWrapper = () => {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  });
  return ({ children }: { children: React.ReactNode }) => (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  );
};

describe("CompareScoreBanner", () => {
  beforeEach(() => {
    mockUseAppSelector.mockImplementation((selector: (s: unknown) => unknown) =>
      selector({ auth: { userId: "user-1" } }),
    );
  });

  it("shows enable scoring message when scoring is disabled", () => {
    const queryClient = new QueryClient({
      defaultOptions: { queries: { retry: false } },
    });
    queryClient.setQueryData(["userPreferences"], {
      data: { enableVehicleScoring: false },
    });

    render(
      <QueryClientProvider client={queryClient}>
        <CompareScoreBanner listingAId="a" listingBId="b" />
      </QueryClientProvider>,
    );
    expect(screen.getByText("score.enableScoring")).toBeInTheDocument();
  });

  it("renders score columns when scoring enabled", async () => {
    render(
      <CompareScoreBanner listingAId="a" listingBId="b" />,
      { wrapper: createWrapper() },
    );
    expect(await screen.findAllByText("75")).toHaveLength(2);
  });

  it("shows customize button for authenticated users", async () => {
    render(
      <CompareScoreBanner listingAId="a" listingBId="b" />,
      { wrapper: createWrapper() },
    );
    expect(await screen.findByText("score.customize")).toBeInTheDocument();
  });

  it("renders factor labels", async () => {
    render(
      <CompareScoreBanner listingAId="a" listingBId="b" />,
      { wrapper: createWrapper() },
    );
    await screen.findAllByText("75");
    expect(screen.getByText("score.value")).toBeInTheDocument();
    expect(screen.getByText("score.efficiency")).toBeInTheDocument();
    expect(screen.getByText("score.reliability")).toBeInTheDocument();
    expect(screen.getByText("score.mileage")).toBeInTheDocument();
    expect(screen.getByText("score.condition")).toBeInTheDocument();
  });
});
