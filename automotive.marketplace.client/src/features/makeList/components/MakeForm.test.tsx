import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import { describe, it, expect, vi, beforeEach } from "vitest";
import MakeForm from "./MakeForm";

vi.mock("react-i18next", () => ({
  useTranslation: () => ({ t: (key: string) => key }),
  initReactI18next: { type: "3rdParty", init: () => {} },
}));

describe("MakeForm", () => {
  const mockOnSubmit = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();
    mockOnSubmit.mockResolvedValue(undefined);
  });

  it("renders name input and submit button", () => {
    render(<MakeForm make={{ name: "" }} onSubmit={mockOnSubmit} />);

    expect(screen.getByLabelText("admin:makes.makeName")).toBeInTheDocument();
    expect(
      screen.getByRole("button", { name: "common:actions.confirm" })
    ).toBeInTheDocument();
  });

  it("renders with pre-filled name value", () => {
    render(<MakeForm make={{ name: "Toyota" }} onSubmit={mockOnSubmit} />);

    expect(screen.getByLabelText("admin:makes.makeName")).toHaveValue("Toyota");
  });

  it("calls onSubmit with form data on valid submit", async () => {
    render(<MakeForm make={{ name: "" }} onSubmit={mockOnSubmit} />);

    fireEvent.change(screen.getByLabelText("admin:makes.makeName"), {
      target: { value: "Honda" },
    });
    fireEvent.click(
      screen.getByRole("button", { name: "common:actions.confirm" })
    );

    await waitFor(() => {
      expect(mockOnSubmit).toHaveBeenCalledWith({ name: "Honda" });
    });
  });

  it("does not call onSubmit when name is empty", async () => {
    render(<MakeForm make={{ name: "" }} onSubmit={mockOnSubmit} />);

    fireEvent.click(
      screen.getByRole("button", { name: "common:actions.confirm" })
    );

    await waitFor(() => {
      expect(mockOnSubmit).not.toHaveBeenCalled();
    });
  });
});
