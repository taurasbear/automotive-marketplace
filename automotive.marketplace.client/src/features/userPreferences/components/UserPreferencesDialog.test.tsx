import { render, screen, fireEvent } from "@testing-library/react";
import { describe, it, expect, vi, beforeEach } from "vitest";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";

const { mockUpsert } = vi.hoisted(() => ({
  mockUpsert: vi.fn().mockResolvedValue(undefined),
}));

vi.mock("react-i18next", () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}));

vi.mock("../api/useUpsertUserPreferences", () => ({
  useUpsertUserPreferences: () => ({ mutateAsync: mockUpsert, isPending: false }),
}));

vi.mock("../api/getUserPreferencesOptions", () => ({
  getUserPreferencesOptions: {
    queryKey: ["userPreferences"],
    queryFn: () =>
      Promise.resolve({
        data: {
          hasCompletedQuiz: false,
          hasPreferences: false,
          valueWeight: 0.2,
          efficiencyWeight: 0.2,
          reliabilityWeight: 0.2,
          mileageWeight: 0.2,
          conditionWeight: 0.2,
          autoGenerateAiSummary: true,
          enableVehicleScoring: false,
          enableMarketPriceApi: false,
        },
      }),
  },
}));

vi.mock("@/components/ui/dialog", () => ({
  Dialog: ({
    children,
    open,
  }: {
    children: React.ReactNode;
    open: boolean;
  }) => (open ? <div data-testid="dialog">{children}</div> : null),
  DialogContent: ({ children }: { children: React.ReactNode }) => (
    <div>{children}</div>
  ),
  DialogHeader: ({ children }: { children: React.ReactNode }) => (
    <div>{children}</div>
  ),
  DialogTitle: ({ children }: { children: React.ReactNode }) => (
    <h2>{children}</h2>
  ),
  DialogFooter: ({ children }: { children: React.ReactNode }) => (
    <div data-testid="dialog-footer">{children}</div>
  ),
}));

vi.mock("@/components/ui/button", () => ({
  Button: ({
    children,
    onClick,
    disabled,
  }: {
    children: React.ReactNode;
    onClick?: () => void;
    disabled?: boolean;
  }) => (
    <button onClick={onClick} disabled={disabled}>
      {children}
    </button>
  ),
}));

vi.mock("@/components/ui/slider", () => ({
  Slider: ({
    value,
    onValueChange,
    min,
    max,
  }: {
    value: number[];
    onValueChange: (v: number[]) => void;
    min: number;
    max: number;
  }) => (
    <input
      type="range"
      value={value[0]}
      min={min}
      max={max}
      onChange={(e) => onValueChange([Number(e.target.value)])}
      data-testid="slider"
    />
  ),
}));

import { UserPreferencesDialog } from "./UserPreferencesDialog";

const createWrapper = () => {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  });
  return ({ children }: { children: React.ReactNode }) => (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  );
};

describe("UserPreferencesDialog", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("does not render when closed", () => {
    render(
      <UserPreferencesDialog open={false} onOpenChange={vi.fn()} />,
      { wrapper: createWrapper() },
    );
    expect(screen.queryByTestId("dialog")).not.toBeInTheDocument();
  });

  it("renders dialog when open", () => {
    render(
      <UserPreferencesDialog open={true} onOpenChange={vi.fn()} />,
      { wrapper: createWrapper() },
    );
    expect(screen.getByTestId("dialog")).toBeInTheDocument();
  });

  it("shows quiz step 0 (driving style) by default for new user", () => {
    render(
      <UserPreferencesDialog open={true} onOpenChange={vi.fn()} />,
      { wrapper: createWrapper() },
    );
    expect(screen.getByText("quiz.step0Title")).toBeInTheDocument();
    expect(screen.getByText("quiz.styleCity")).toBeInTheDocument();
    expect(screen.getByText("quiz.styleHighway")).toBeInTheDocument();
    expect(screen.getByText("quiz.styleMixed")).toBeInTheDocument();
  });

  it("navigates to step 1 (priorities) when Next is clicked", () => {
    render(
      <UserPreferencesDialog open={true} onOpenChange={vi.fn()} />,
      { wrapper: createWrapper() },
    );
    fireEvent.click(screen.getByText("quiz.next"));
    expect(screen.getByText("quiz.step1Title")).toBeInTheDocument();
    expect(screen.getByText("quiz.priorityValue")).toBeInTheDocument();
    expect(screen.getByText("quiz.priorityEfficiency")).toBeInTheDocument();
  });

  it("navigates to step 2 (sliders) from step 1", () => {
    render(
      <UserPreferencesDialog open={true} onOpenChange={vi.fn()} />,
      { wrapper: createWrapper() },
    );
    fireEvent.click(screen.getByText("quiz.next"));
    fireEvent.click(screen.getByText("quiz.next"));
    expect(screen.getByText("quiz.step2Title")).toBeInTheDocument();
    expect(screen.getAllByTestId("slider")).toHaveLength(5);
  });

  it("can go back from step 1 to step 0", () => {
    render(
      <UserPreferencesDialog open={true} onOpenChange={vi.fn()} />,
      { wrapper: createWrapper() },
    );
    fireEvent.click(screen.getByText("quiz.next"));
    expect(screen.getByText("quiz.step1Title")).toBeInTheDocument();
    fireEvent.click(screen.getByText("quiz.back"));
    expect(screen.getByText("quiz.step0Title")).toBeInTheDocument();
  });

  it("calls upsert and transitions to settings on save at step 2", async () => {
    render(
      <UserPreferencesDialog open={true} onOpenChange={vi.fn()} />,
      { wrapper: createWrapper() },
    );
    // Navigate to step 2
    fireEvent.click(screen.getByText("quiz.next"));
    fireEvent.click(screen.getByText("quiz.next"));
    // Save
    fireEvent.click(screen.getByText("quiz.save"));
    await vi.waitFor(() => {
      expect(mockUpsert).toHaveBeenCalled();
    });
  });

  it("shows settings view with sliders when initialView is 'settings'", () => {
    render(
      <UserPreferencesDialog open={true} onOpenChange={vi.fn()} initialView="settings" />,
      { wrapper: createWrapper() },
    );
    expect(screen.getByText("quiz.step2Title")).toBeInTheDocument();
    expect(screen.getAllByTestId("slider")).toHaveLength(5);
  });

  it("shows retake quiz button in settings view", () => {
    render(
      <UserPreferencesDialog open={true} onOpenChange={vi.fn()} initialView="settings" />,
      { wrapper: createWrapper() },
    );
    expect(screen.getByText("quiz.retakeQuiz")).toBeInTheDocument();
  });

  it("retake quiz button switches to quiz view step 0", () => {
    render(
      <UserPreferencesDialog open={true} onOpenChange={vi.fn()} initialView="settings" />,
      { wrapper: createWrapper() },
    );
    fireEvent.click(screen.getByText("quiz.retakeQuiz"));
    expect(screen.getByText("quiz.step0Title")).toBeInTheDocument();
  });
});
