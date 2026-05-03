import { render, screen, fireEvent } from "@testing-library/react";
import { describe, it, expect, vi, beforeEach } from "vitest";
import EditableField from "./EditableField";

vi.mock("@/lib/i18n/formatNumber", () => ({
  formatNumber: (v: number) => v.toLocaleString(),
}));

describe("EditableField", () => {
  const defaultProps = {
    label: "Price",
    value: 25000,
    type: "number" as const,
    onConfirm: vi.fn(),
  };

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("renders display mode with label and value", () => {
    render(<EditableField {...defaultProps} />);
    expect(screen.getByText("Price")).toBeInTheDocument();
    expect(screen.getByText("25,000")).toBeInTheDocument();
  });

  it("renders custom displayValue when provided", () => {
    render(<EditableField {...defaultProps} displayValue="€25,000" />);
    expect(screen.getByText("€25,000")).toBeInTheDocument();
  });

  it("shows pending change with strikethrough and amber value", () => {
    render(<EditableField {...defaultProps} pendingValue={30000} />);
    expect(screen.getByText("25,000")).toBeInTheDocument();
    expect(screen.getByText("30,000")).toBeInTheDocument();
  });

  it("clicking edit button enters edit mode with input", () => {
    render(<EditableField {...defaultProps} />);
    fireEvent.click(screen.getByRole("button"));
    expect(screen.getByRole("spinbutton")).toBeInTheDocument();
  });

  it("confirm button calls onConfirm with new value", () => {
    const onConfirm = vi.fn();
    render(<EditableField {...defaultProps} onConfirm={onConfirm} />);
    fireEvent.click(screen.getByRole("button"));

    const input = screen.getByRole("spinbutton");
    fireEvent.change(input, { target: { value: "30000" } });

    // Click confirm (first button with Check icon)
    const buttons = screen.getAllByRole("button");
    fireEvent.click(buttons[0]); // confirm button
    expect(onConfirm).toHaveBeenCalledWith(30000);
  });

  it("cancel button reverts to display mode", () => {
    render(<EditableField {...defaultProps} />);
    fireEvent.click(screen.getByRole("button"));

    // Click cancel (second button)
    const buttons = screen.getAllByRole("button");
    fireEvent.click(buttons[1]); // cancel button
    expect(screen.queryByRole("spinbutton")).not.toBeInTheDocument();
    expect(screen.getByText("25,000")).toBeInTheDocument();
  });

  it("Enter key confirms edit", () => {
    const onConfirm = vi.fn();
    render(<EditableField {...defaultProps} onConfirm={onConfirm} />);
    fireEvent.click(screen.getByRole("button"));

    const input = screen.getByRole("spinbutton");
    fireEvent.change(input, { target: { value: "28000" } });
    fireEvent.keyDown(input, { key: "Enter" });
    expect(onConfirm).toHaveBeenCalledWith(28000);
  });

  it("Escape key cancels edit", () => {
    render(<EditableField {...defaultProps} />);
    fireEvent.click(screen.getByRole("button"));

    const input = screen.getByRole("spinbutton");
    fireEvent.keyDown(input, { key: "Escape" });
    expect(screen.queryByRole("spinbutton")).not.toBeInTheDocument();
  });

  it("renders text input for type text", () => {
    render(
      <EditableField
        label="Colour"
        value="Red"
        type="text"
        onConfirm={vi.fn()}
      />,
    );
    expect(screen.getByText("Red")).toBeInTheDocument();
    fireEvent.click(screen.getByRole("button"));
    expect(screen.getByRole("textbox")).toHaveValue("Red");
  });

  it("renders textarea for type textarea", () => {
    render(
      <EditableField
        label="Description"
        value="Nice car"
        type="textarea"
        onConfirm={vi.fn()}
      />,
    );
    fireEvent.click(screen.getByRole("button"));
    expect(screen.getByRole("textbox")).toHaveValue("Nice car");
  });

  it("renders toggle for type toggle with labels", () => {
    render(
      <EditableField
        label="Condition"
        value={true}
        type="toggle"
        toggleLabels={{ on: "Used", off: "New" }}
        onConfirm={vi.fn()}
      />,
    );
    expect(screen.getByText("Used")).toBeInTheDocument();
  });

  it("uses pendingValue as initial edit value when present", () => {
    render(<EditableField {...defaultProps} pendingValue={30000} />);
    fireEvent.click(screen.getAllByRole("button")[0]);
    expect(screen.getByRole("spinbutton")).toHaveValue(30000);
  });
});
