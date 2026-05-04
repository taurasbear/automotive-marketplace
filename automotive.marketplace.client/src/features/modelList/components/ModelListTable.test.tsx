import { render, screen, fireEvent } from "@testing-library/react";
import { describe, it, expect, vi, beforeEach } from "vitest";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import ModelListTable from "./ModelListTable";

const { mockDeleteModelAsync } = vi.hoisted(() => ({
  mockDeleteModelAsync: vi.fn(),
}));

vi.mock("react-i18next", () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}));

vi.mock("@/lib/i18n/dateLocale", () => ({
  useDateLocale: () => undefined,
}));

vi.mock("../api/getAllModelsOptions", () => ({
  getAllModelsOptions: {
    queryKey: ["models"],
    queryFn: () =>
      Promise.resolve({
        data: [
          {
            id: "model-1",
            name: "Corolla",
            createdBy: "admin",
            createdAt: "2024-01-15T00:00:00Z",
            modifiedAt: null,
            modifiedBy: "",
          },
          {
            id: "model-2",
            name: "Civic",
            createdBy: "admin",
            createdAt: "2024-02-15T00:00:00Z",
            modifiedAt: null,
            modifiedBy: "",
          },
        ],
      }),
  },
}));

vi.mock("../api/useDeleteModel", () => ({
  useDeleteModel: () => ({ mutateAsync: mockDeleteModelAsync }),
}));

vi.mock("./EditModelDialog", () => ({
  default: ({ id }: { id: string }) => (
    <button data-testid={`edit-${id}`}>Edit</button>
  ),
}));

vi.mock("./ViewModelDialog", () => ({
  default: ({ id }: { id: string }) => (
    <button data-testid={`view-${id}`}>View</button>
  ),
}));

const createWrapper = () => {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  });
  return ({ children }: { children: React.ReactNode }) => (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  );
};

describe("ModelListTable", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockDeleteModelAsync.mockResolvedValue(undefined);
  });

  it("renders table headers", () => {
    render(<ModelListTable />, { wrapper: createWrapper() });

    expect(screen.getByText("models.name")).toBeInTheDocument();
    expect(screen.getByText("models.createdBy")).toBeInTheDocument();
    expect(screen.getByText("models.createdAt")).toBeInTheDocument();
    expect(screen.getByText("models.actions")).toBeInTheDocument();
  });

  it("renders model rows after data loads", async () => {
    render(<ModelListTable />, { wrapper: createWrapper() });

    expect(await screen.findByText("Corolla")).toBeInTheDocument();
    expect(screen.getByText("Civic")).toBeInTheDocument();
  });

  it("renders view and edit buttons for each model", async () => {
    render(<ModelListTable />, { wrapper: createWrapper() });

    expect(await screen.findByTestId("view-model-1")).toBeInTheDocument();
    expect(screen.getByTestId("edit-model-1")).toBeInTheDocument();
    expect(screen.getByTestId("view-model-2")).toBeInTheDocument();
    expect(screen.getByTestId("edit-model-2")).toBeInTheDocument();
  });

  it("calls deleteModelAsync when delete button is clicked", async () => {
    render(<ModelListTable />, { wrapper: createWrapper() });

    await screen.findByText("Corolla");
    const deleteButtons = screen.getAllByRole("button", { name: "" });
    fireEvent.click(deleteButtons[0]);

    expect(mockDeleteModelAsync).toHaveBeenCalledWith({ id: "model-1" });
  });
});
