import { render, screen, fireEvent } from "@testing-library/react";
import { describe, it, expect, vi, beforeEach } from "vitest";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import MakeListTable from "./MakeListTable";

const { mockDeleteMakeAsync } = vi.hoisted(() => ({
  mockDeleteMakeAsync: vi.fn(),
}));

vi.mock("react-i18next", () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}));

vi.mock("@/lib/i18n/dateLocale", () => ({
  useDateLocale: () => undefined,
}));

vi.mock("@/api/make/getAllMakesOptions", () => ({
  getAllMakesOptions: {
    queryKey: ["makes"],
    queryFn: () =>
      Promise.resolve({
        data: [
          {
            id: "make-1",
            name: "Toyota",
            createdBy: "admin",
            createdAt: "2024-01-01T00:00:00Z",
            modifiedAt: null,
            modifiedBy: "",
          },
          {
            id: "make-2",
            name: "Honda",
            createdBy: "admin",
            createdAt: "2024-02-01T00:00:00Z",
            modifiedAt: null,
            modifiedBy: "",
          },
        ],
      }),
  },
}));

vi.mock("../api/useDeleteMake", () => ({
  useDeleteMake: () => ({ mutateAsync: mockDeleteMakeAsync }),
}));

vi.mock("./EditMakeDialog", () => ({
  default: ({ id }: { id: string }) => (
    <button data-testid={`edit-${id}`}>Edit</button>
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

describe("MakeListTable", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockDeleteMakeAsync.mockResolvedValue(undefined);
  });

  it("renders table headers", async () => {
    render(<MakeListTable />, { wrapper: createWrapper() });

    expect(screen.getByText("admin:makes.name")).toBeInTheDocument();
    expect(screen.getByText("admin:makes.createdBy")).toBeInTheDocument();
    expect(screen.getByText("admin:makes.createdAt")).toBeInTheDocument();
    expect(screen.getByText("admin:makes.actions")).toBeInTheDocument();
  });

  it("renders make rows after data loads", async () => {
    render(<MakeListTable />, { wrapper: createWrapper() });

    expect(await screen.findByText("Toyota")).toBeInTheDocument();
    expect(screen.getByText("Honda")).toBeInTheDocument();
  });

  it("renders edit button for each make", async () => {
    render(<MakeListTable />, { wrapper: createWrapper() });

    expect(await screen.findByTestId("edit-make-1")).toBeInTheDocument();
    expect(screen.getByTestId("edit-make-2")).toBeInTheDocument();
  });

  it("shows delete confirmation dialog on delete click", async () => {
    render(<MakeListTable />, { wrapper: createWrapper() });

    await screen.findByText("Toyota");
    const deleteButtons = screen.getAllByRole("button", { name: "" });
    fireEvent.click(deleteButtons[0]);

    expect(
      await screen.findByText("admin:makes.deleteWarning")
    ).toBeInTheDocument();
  });

  it("calls deleteMakeAsync when delete is confirmed", async () => {
    render(<MakeListTable />, { wrapper: createWrapper() });

    await screen.findByText("Toyota");
    const deleteButtons = screen.getAllByRole("button", { name: "" });
    fireEvent.click(deleteButtons[0]);

    const confirmBtn = await screen.findByText("common:actions.delete");
    fireEvent.click(confirmBtn);

    expect(mockDeleteMakeAsync).toHaveBeenCalledWith({ id: "make-1" });
  });
});
