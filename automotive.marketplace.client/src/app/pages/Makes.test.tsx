import { render, screen } from "@testing-library/react";
import { describe, it, expect, vi } from "vitest";
import Makes from "./Makes";

vi.mock("@/features/makeList", () => ({
  CreateMakeDialog: () => (
    <button data-testid="create-make-dialog">Add Make</button>
  ),
  MakeListTable: ({ className }: { className?: string }) => (
    <div data-testid="make-list-table" className={className}>
      Table
    </div>
  ),
}));

describe("Makes page", () => {
  it("renders CreateMakeDialog and MakeListTable", () => {
    render(<Makes />);

    expect(screen.getByTestId("create-make-dialog")).toBeInTheDocument();
    expect(screen.getByTestId("make-list-table")).toBeInTheDocument();
  });

  it("applies max-w-3xl class to MakeListTable", () => {
    render(<Makes />);

    expect(screen.getByTestId("make-list-table")).toHaveClass("max-w-3xl");
  });
});
