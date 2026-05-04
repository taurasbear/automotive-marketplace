import { render, screen, fireEvent } from "@testing-library/react";
import { describe, it, expect, vi, beforeEach } from "vitest";
import Variants from "./Variants";

const { mockMakeSelect, mockModelSelect } = vi.hoisted(() => ({
  mockMakeSelect: vi.fn(),
  mockModelSelect: vi.fn(),
}));

vi.mock("@/components/forms/select/MakeSelect", () => ({
  default: (props: {
    onValueChange: (v: string) => void;
    value: string;
  }) => {
    mockMakeSelect(props);
    return (
      <select
        data-testid="make-select"
        value={props.value}
        onChange={(e) => props.onValueChange(e.target.value)}
      >
        <option value="">Select Make</option>
        <option value="make-1">Toyota</option>
      </select>
    );
  },
}));

vi.mock("@/components/forms/select/ModelSelect", () => ({
  default: (props: {
    onValueChange: (v: string) => void;
    value: string;
    selectedMake?: string;
  }) => {
    mockModelSelect(props);
    return (
      <select
        data-testid="model-select"
        value={props.value}
        onChange={(e) => props.onValueChange(e.target.value)}
      >
        <option value="">Select Model</option>
        <option value="model-1">Corolla</option>
      </select>
    );
  },
}));

vi.mock("@/features/variantList", () => ({
  CreateVariantDialog: ({
    modelId,
    makeId,
  }: {
    modelId: string;
    makeId: string;
  }) => (
    <button data-testid="create-variant-dialog">
      Add Variant {makeId} {modelId}
    </button>
  ),
  VariantListTable: ({
    modelId,
    makeId,
  }: {
    modelId: string;
    makeId: string;
    className?: string;
  }) => (
    <div data-testid="variant-list-table">
      Table {makeId} {modelId}
    </div>
  ),
}));

describe("Variants page", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("renders make and model selects", () => {
    render(<Variants />);

    expect(screen.getByTestId("make-select")).toBeInTheDocument();
    expect(screen.getByTestId("model-select")).toBeInTheDocument();
  });

  it("does not show create button or table initially", () => {
    render(<Variants />);

    expect(
      screen.queryByTestId("create-variant-dialog")
    ).not.toBeInTheDocument();
    expect(screen.queryByTestId("variant-list-table")).not.toBeInTheDocument();
  });

  it("shows create button and table when both make and model are selected", () => {
    render(<Variants />);

    fireEvent.change(screen.getByTestId("make-select"), {
      target: { value: "make-1" },
    });
    fireEvent.change(screen.getByTestId("model-select"), {
      target: { value: "model-1" },
    });

    expect(screen.getByTestId("create-variant-dialog")).toBeInTheDocument();
    expect(screen.getByTestId("variant-list-table")).toBeInTheDocument();
  });

  it("hides table when only make is selected", () => {
    render(<Variants />);

    fireEvent.change(screen.getByTestId("make-select"), {
      target: { value: "make-1" },
    });

    expect(
      screen.queryByTestId("create-variant-dialog")
    ).not.toBeInTheDocument();
    expect(screen.queryByTestId("variant-list-table")).not.toBeInTheDocument();
  });
});
