import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import { describe, it, expect, vi, beforeEach } from "vitest";
import ModelForm from "./ModelForm";

vi.mock("react-i18next", () => ({
  useTranslation: () => ({ t: (key: string) => key }),
  initReactI18next: { type: "3rdParty", init: () => {} },
}));

vi.mock("@/components/forms/select/MakeSelect", () => ({
  default: ({
    onValueChange,
    value,
  }: {
    onValueChange: (v: string) => void;
    value?: string;
  }) => (
    <select
      data-testid="make-select"
      value={value}
      onChange={(e) => onValueChange(e.target.value)}
    >
      <option value="">Select</option>
      <option value="make-1">Toyota</option>
    </select>
  ),
}));

describe("ModelForm", () => {
  const mockOnSubmit = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();
    mockOnSubmit.mockResolvedValue(undefined);
  });

  it("renders name input and make select", () => {
    render(
      <ModelForm model={{ name: "", makeId: "" }} onSubmit={mockOnSubmit} />
    );

    expect(screen.getByLabelText("admin:models.modelName")).toBeInTheDocument();
    expect(screen.getByTestId("make-select")).toBeInTheDocument();
  });

  it("renders with pre-filled values", () => {
    render(
      <ModelForm
        model={{ name: "Corolla", makeId: "make-1" }}
        onSubmit={mockOnSubmit}
      />
    );

    expect(screen.getByLabelText("admin:models.modelName")).toHaveValue(
      "Corolla"
    );
  });

  it("renders submit button", () => {
    render(
      <ModelForm model={{ name: "", makeId: "" }} onSubmit={mockOnSubmit} />
    );

    expect(
      screen.getByRole("button", { name: "common:actions.confirm" })
    ).toBeInTheDocument();
  });

  it("calls onSubmit with form data on valid submit", async () => {
    render(
      <ModelForm
        model={{ name: "Corolla", makeId: "make-1" }}
        onSubmit={mockOnSubmit}
      />
    );

    fireEvent.click(
      screen.getByRole("button", { name: "common:actions.confirm" })
    );

    await waitFor(() => {
      expect(mockOnSubmit).toHaveBeenCalledWith({
        name: "Corolla",
        makeId: "make-1",
      });
    });
  });

  it("does not call onSubmit when name is empty", async () => {
    render(
      <ModelForm model={{ name: "", makeId: "make-1" }} onSubmit={mockOnSubmit} />
    );

    fireEvent.click(
      screen.getByRole("button", { name: "common:actions.confirm" })
    );

    await waitFor(() => {
      expect(mockOnSubmit).not.toHaveBeenCalled();
    });
  });
});
