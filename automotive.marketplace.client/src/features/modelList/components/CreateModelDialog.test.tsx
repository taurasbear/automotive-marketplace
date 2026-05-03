import { render, screen, fireEvent } from "@testing-library/react";
import { describe, it, expect, vi, beforeEach } from "vitest";
import CreateModelDialog from "./CreateModelDialog";

const { mockCreateModelAsync } = vi.hoisted(() => ({
  mockCreateModelAsync: vi.fn(),
}));

vi.mock("react-i18next", () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}));

vi.mock("../api/useCreateModel", () => ({
  useCreateModel: () => ({ mutateAsync: mockCreateModelAsync }),
}));

vi.mock("./ModelForm", () => ({
  default: ({
    model,
    onSubmit,
  }: {
    model: { name: string; makeId: string };
    onSubmit: (data: { name: string; makeId: string }) => void;
  }) => (
    <div data-testid="model-form">
      <span data-testid="form-name">{model.name}</span>
      <span data-testid="form-makeId">{model.makeId}</span>
      <button
        data-testid="form-submit"
        onClick={() => onSubmit({ name: "NewModel", makeId: "make-1" })}
      >
        Submit
      </button>
    </div>
  ),
}));

describe("CreateModelDialog", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockCreateModelAsync.mockResolvedValue(undefined);
  });

  it("renders trigger button", () => {
    render(<CreateModelDialog />);

    expect(
      screen.getByRole("button", { name: "models.addModel" })
    ).toBeInTheDocument();
  });

  it("opens dialog when trigger is clicked", () => {
    render(<CreateModelDialog />);

    fireEvent.click(screen.getByRole("button", { name: "models.addModel" }));

    expect(screen.getByText("models.createNewModel")).toBeInTheDocument();
  });

  it("renders ModelForm with empty values inside dialog", () => {
    render(<CreateModelDialog />);

    fireEvent.click(screen.getByRole("button", { name: "models.addModel" }));

    expect(screen.getByTestId("model-form")).toBeInTheDocument();
    expect(screen.getByTestId("form-name")).toHaveTextContent("");
    expect(screen.getByTestId("form-makeId")).toHaveTextContent("");
  });

  it("calls createModelAsync on form submit", async () => {
    render(<CreateModelDialog />);

    fireEvent.click(screen.getByRole("button", { name: "models.addModel" }));
    fireEvent.click(screen.getByTestId("form-submit"));

    expect(mockCreateModelAsync).toHaveBeenCalledWith({
      name: "NewModel",
      makeId: "make-1",
    });
  });
});
