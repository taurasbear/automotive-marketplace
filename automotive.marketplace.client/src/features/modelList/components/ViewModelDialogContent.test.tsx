import { render, screen } from "@testing-library/react";
import { describe, it, expect, vi, beforeEach } from "vitest";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { Suspense } from "react";
import ViewModelDialogContent from "./ViewModelDialogContent";

vi.mock("react-i18next", () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}));

vi.mock("@/lib/i18n/dateLocale", () => ({
  useDateLocale: () => undefined,
}));

vi.mock("@/components/ui/dialog", () => ({
  DialogHeader: ({ children }: { children: React.ReactNode }) => (
    <div data-testid="dialog-header">{children}</div>
  ),
  DialogTitle: ({ children }: { children: React.ReactNode }) => (
    <h2 data-testid="dialog-title">{children}</h2>
  ),
  DialogDescription: ({ children }: { children: React.ReactNode }) => (
    <p data-testid="dialog-description">{children}</p>
  ),
}));

const mockModelData = {
  id: "model-1",
  name: "Corolla",
  makeId: "make-1",
  createdAt: "2024-01-15T10:00:00Z",
  modifiedAt: "2024-03-01T12:00:00Z",
  createdBy: "admin",
  modifiedBy: "editor",
};

vi.mock("../api/getModelByIdOptions", () => ({
  getModelByIdOptions: () => ({
    queryKey: ["model", "model-1"],
    queryFn: () => Promise.resolve({ data: mockModelData }),
  }),
}));

const createWrapper = () => {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  });
  queryClient.setQueryData(["model", "model-1"], { data: mockModelData });
  return ({ children }: { children: React.ReactNode }) => (
    <QueryClientProvider client={queryClient}>
      <Suspense fallback={<div>Loading...</div>}>{children}</Suspense>
    </QueryClientProvider>
  );
};

describe("ViewModelDialogContent", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("renders dialog title", async () => {
    render(<ViewModelDialogContent id="model-1" />, {
      wrapper: createWrapper(),
    });

    expect(await screen.findByText("models.modelDetails")).toBeInTheDocument();
  });

  it("renders model name", async () => {
    render(<ViewModelDialogContent id="model-1" />, {
      wrapper: createWrapper(),
    });

    expect(await screen.findByText("Corolla")).toBeInTheDocument();
  });

  it("renders created by info", async () => {
    render(<ViewModelDialogContent id="model-1" />, {
      wrapper: createWrapper(),
    });

    await screen.findByText("Corolla");
    expect(screen.getByText(/admin/)).toBeInTheDocument();
  });

  it("renders modified info when modifiedAt exists", async () => {
    render(<ViewModelDialogContent id="model-1" />, {
      wrapper: createWrapper(),
    });

    await screen.findByText("Corolla");
    expect(screen.getByText(/editor/)).toBeInTheDocument();
  });
});
