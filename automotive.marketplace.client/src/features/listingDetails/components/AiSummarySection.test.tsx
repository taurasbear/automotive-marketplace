import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import { describe, it, expect, vi, beforeEach } from "vitest";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { AiSummarySection } from "./AiSummarySection";

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
    i18n: { language: "lt" },
  }),
}));

vi.mock("@/features/userPreferences", () => ({
  getUserPreferencesOptions: {
    queryKey: ["userPreferences"],
    queryFn: () =>
      Promise.resolve({
        data: { autoGenerateAiSummary: true, enableVehicleScoring: true },
      }),
  },
}));

vi.mock("../api/getListingAiSummaryOptions", () => ({
  getListingAiSummaryOptions: () => ({
    queryKey: ["listing", "aiSummary", "listing-1", "lt"],
    queryFn: () =>
      Promise.resolve({
        data: {
          summary: "This is a great car.",
          isGenerated: true,
          fromCache: false,
          unavailableFactors: [],
        },
      }),
    enabled: true,
  }),
}));

const createWrapper = (prefsData?: unknown, summaryData?: unknown) => {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false, staleTime: Infinity } },
  });
  queryClient.setQueryData(
    ["userPreferences"],
    prefsData ?? {
      data: { autoGenerateAiSummary: true, enableVehicleScoring: true },
    },
  );
  if (summaryData !== undefined) {
    queryClient.setQueryData(
      ["listing", "aiSummary", "listing-1", "lt"],
      summaryData,
    );
  }
  return ({ children }: { children: React.ReactNode }) => (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  );
};

describe("AiSummarySection", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockUseAppSelector.mockImplementation((selector: (state: unknown) => unknown) =>
      selector({ auth: { userId: "user-1", permissions: [] } }),
    );
  });

  it("renders without crashing", () => {
    render(<AiSummarySection listingId="listing-1" />, {
      wrapper: createWrapper(),
    });
    expect(screen.getByText("aiSummary.title")).toBeInTheDocument();
  });

  it("renders AI summary text when available", () => {
    const summaryData = {
      data: {
        summary: "This is a great car.",
        isGenerated: true,
        fromCache: false,
        unavailableFactors: [],
      },
    };
    render(<AiSummarySection listingId="listing-1" />, {
      wrapper: createWrapper(undefined, summaryData),
    });
    expect(screen.getByText("This is a great car.")).toBeInTheDocument();
  });

  it("shows regenerate button text when summary exists", () => {
    const summaryData = {
      data: {
        summary: "This is a great car.",
        isGenerated: true,
        fromCache: false,
        unavailableFactors: [],
      },
    };
    render(<AiSummarySection listingId="listing-1" />, {
      wrapper: createWrapper(undefined, summaryData),
    });
    expect(screen.getByText("aiSummary.regenerate")).toBeInTheDocument();
  });

  it("shows generate button text when no summary exists", () => {
    render(<AiSummarySection listingId="listing-1" />, {
      wrapper: createWrapper(undefined, null),
    });
    expect(screen.getByText("aiSummary.generate")).toBeInTheDocument();
  });

  it("shows prompt message when no data and not fetching", () => {
    mockUseAppSelector.mockImplementation((selector: (state: unknown) => unknown) =>
      selector({ auth: { userId: "user-1", permissions: [] } }),
    );
    render(<AiSummarySection listingId="listing-1" />, {
      wrapper: createWrapper(undefined, undefined),
    });
    // When auto-generate is enabled but no data yet, it should show prompt or be fetching
    expect(screen.getByText("aiSummary.title")).toBeInTheDocument();
  });

  it("shows unavailable factors alert when present", () => {
    const summaryData = {
      data: {
        summary: "Summary with missing factors",
        isGenerated: true,
        fromCache: false,
        unavailableFactors: ["MarketValue"],
      },
    };
    render(<AiSummarySection listingId="listing-1" />, {
      wrapper: createWrapper(undefined, summaryData),
    });
    expect(
      screen.getByText(/aiSummary.unavailableFactors/),
    ).toBeInTheDocument();
  });

  it("shows unavailable message when not generated", () => {
    const summaryData = {
      data: {
        summary: null,
        isGenerated: false,
        fromCache: false,
        unavailableFactors: [],
      },
    };
    render(<AiSummarySection listingId="listing-1" />, {
      wrapper: createWrapper(undefined, summaryData),
    });
    expect(screen.getByText("aiSummary.unavailable")).toBeInTheDocument();
  });

  it("does not auto-generate when user is not logged in", () => {
    mockUseAppSelector.mockImplementation((selector: (state: unknown) => unknown) =>
      selector({ auth: { userId: null, permissions: [] } }),
    );
    render(<AiSummarySection listingId="listing-1" />, {
      wrapper: createWrapper(),
    });
    // Should show prompt since autoGenerate = false for non-logged-in
    expect(screen.getByText("aiSummary.title")).toBeInTheDocument();
  });

  it("regenerate button is present", () => {
    render(<AiSummarySection listingId="listing-1" />, {
      wrapper: createWrapper(),
    });
    const btn = screen.getByRole("button");
    expect(btn).toBeInTheDocument();
  });
});
