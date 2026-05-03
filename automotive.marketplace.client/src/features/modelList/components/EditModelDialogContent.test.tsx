import { render, screen } from "@testing-library/react";
import { describe, it, expect, vi, beforeEach } from "vitest";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { Suspense } from "react";
import EditModelDialogContent from "./EditModelDialogContent";

vi.mock("react-i18next", () => ({
  useTranslation: () => ({ t: (key: string, opts?: object) => `${key} ${JSON.stringify(opts ?? {})}` }),
}));

vi.mock("@/components/ui/dialog", () => ({
  DialogHeader: ({ children }: { children: React.ReactNode }) => (
    <div data-testid="dialog-header">{children}</div>
  ),
  DialogTitle: ({ children }: { children: React.ReactNode }) => (
    <h2 data-testid="dialog-title">{children}</h2>
  ),
}));

vi.mock("./ModelForm", () => ({
  default: ({
    model,
    onSubmit,
  }: {
    model: { name: string; makeId: string };
    onSubmit: (data: unknown) => void;
  }) => (
    <div data-testid="model-form">
      <span data-testid="form-name">{model.name}</span>
      <span data-testid="form-makeId">{model.makeId}</span>
      <button
        data-testid="form-submit"
        onClick={() => onSubmit({ name: "Updated", makeId: "m1" })}
      >
        Submit
      </button>
    </div>
  ),
}));

const mockModelData = {
  id: "model-1",
  name: "Corolla",
  makeId: "make-1",
  createdAt: "2024-01-01T00:00:00Z",
  modifiedAt: null,
  createdBy: "admin",
  modifiedBy: "",
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

describe("EditModelDialogContent", () => {
  const mockOnSubmit = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();
    mockOnSubmit.mockResolvedValue(undefined);
  });

  it("renders dialog title with model name", async () => {
    render(<EditModelDialogContent id="model-1" onSubmit={mockOnSubmit} />, {
      wrapper: createWrapper(),
    });

    expect(await screen.findByTestId("model-form")).toBeInTheDocument();
    expect(screen.getByText(/models.editModel/)).toBeInTheDocument();
  });

  it("renders ModelForm with pre-filled data", async () => {
    render(<EditModelDialogContent id="model-1" onSubmit={mockOnSubmit} />, {
      wrapper: createWrapper(),
    });

    expect(await screen.findByTestId("form-name")).toHaveTextContent("Corolla");
    expect(screen.getByTestId("form-makeId")).toHaveTextContent("make-1");
  });
});
