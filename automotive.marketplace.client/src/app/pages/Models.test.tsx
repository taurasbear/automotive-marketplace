import { render, screen } from "@testing-library/react";
import { describe, it, expect, vi } from "vitest";
import Models from "./Models";

vi.mock("@/features/modelList", () => ({
  CreateModelDialog: () => (
    <button data-testid="create-model-dialog">Add Model</button>
  ),
  ModelListTable: ({ className }: { className?: string }) => (
    <div data-testid="model-list-table" className={className}>
      Table
    </div>
  ),
}));

describe("Models page", () => {
  it("renders CreateModelDialog and ModelListTable", () => {
    render(<Models />);

    expect(screen.getByTestId("create-model-dialog")).toBeInTheDocument();
    expect(screen.getByTestId("model-list-table")).toBeInTheDocument();
  });

  it("applies max-w-3xl class to ModelListTable", () => {
    render(<Models />);

    expect(screen.getByTestId("model-list-table")).toHaveClass("max-w-3xl");
  });
});
