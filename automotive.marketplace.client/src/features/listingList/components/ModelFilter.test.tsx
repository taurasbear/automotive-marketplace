import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, it, expect, vi, beforeEach } from "vitest";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import ModelFilter from "./ModelFilter";

const { mockGetModelsByMakeIdOptions } = vi.hoisted(() => ({
  mockGetModelsByMakeIdOptions: vi.fn(),
}));

vi.mock("@/api/model/getModelsByMakeIdOptions", () => ({
  getModelsByMakeIdOptions: (...args: unknown[]) =>
    mockGetModelsByMakeIdOptions(...args),
}));

const mockModels = [
  { id: "model-1", name: "A3" },
  { id: "model-2", name: "A4" },
  { id: "model-3", name: "Q5" },
];

const createWrapper = () => {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  });
  return ({ children }: { children: React.ReactNode }) => (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  );
};

beforeEach(() => {
  mockGetModelsByMakeIdOptions.mockReturnValue({
    queryKey: ["models", "make-1"],
    queryFn: () => Promise.resolve({ data: mockModels }),
    enabled: true,
  });
});

describe("ModelFilter", () => {
  it("renders model checkboxes when data is available", async () => {
    render(
      <ModelFilter
        makeId="make-1"
        filteredModels={[]}
        onFilterChange={vi.fn()}
      />,
      { wrapper: createWrapper() },
    );

    expect(await screen.findByText("A3")).toBeInTheDocument();
    expect(screen.getByText("A4")).toBeInTheDocument();
    expect(screen.getByText("Q5")).toBeInTheDocument();
  });

  it("checks checkboxes for filtered models", async () => {
    render(
      <ModelFilter
        makeId="make-1"
        filteredModels={["model-1", "model-3"]}
        onFilterChange={vi.fn()}
      />,
      { wrapper: createWrapper() },
    );

    await screen.findByText("A3");

    const checkboxes = screen.getAllByRole("checkbox");
    expect(checkboxes[0]).toHaveAttribute("data-state", "checked");
    expect(checkboxes[1]).toHaveAttribute("data-state", "unchecked");
    expect(checkboxes[2]).toHaveAttribute("data-state", "checked");
  });

  it("calls onFilterChange with model added when checkbox is checked", async () => {
    const user = userEvent.setup();
    const mockOnFilterChange = vi.fn();

    render(
      <ModelFilter
        makeId="make-1"
        filteredModels={["model-1"]}
        onFilterChange={mockOnFilterChange}
      />,
      { wrapper: createWrapper() },
    );

    await screen.findByText("A4");
    const checkboxes = screen.getAllByRole("checkbox");
    await user.click(checkboxes[1]);

    expect(mockOnFilterChange).toHaveBeenCalledWith(["model-1", "model-2"]);
  });

  it("calls onFilterChange with model removed when checkbox is unchecked", async () => {
    const user = userEvent.setup();
    const mockOnFilterChange = vi.fn();

    render(
      <ModelFilter
        makeId="make-1"
        filteredModels={["model-1", "model-2"]}
        onFilterChange={mockOnFilterChange}
      />,
      { wrapper: createWrapper() },
    );

    await screen.findByText("A3");
    const checkboxes = screen.getAllByRole("checkbox");
    await user.click(checkboxes[0]);

    expect(mockOnFilterChange).toHaveBeenCalledWith(["model-2"]);
  });

  it("renders nothing when no models are returned", () => {
    mockGetModelsByMakeIdOptions.mockReturnValue({
      queryKey: ["models", "make-empty"],
      queryFn: () => Promise.resolve({ data: [] }),
      enabled: true,
    });

    const { container } = render(
      <ModelFilter
        makeId="make-empty"
        filteredModels={[]}
        onFilterChange={vi.fn()}
      />,
      { wrapper: createWrapper() },
    );

    expect(container.querySelectorAll("[role='checkbox']")).toHaveLength(0);
  });
});
