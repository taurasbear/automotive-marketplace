import { render, screen, fireEvent } from "@testing-library/react";
import { describe, it, expect, vi, beforeEach } from "vitest";
import CreateMakeDialog from "./CreateMakeDialog";

const { mockCreateMakeAsync } = vi.hoisted(() => ({
  mockCreateMakeAsync: vi.fn(),
}));

vi.mock("react-i18next", () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}));

vi.mock("../api/useCreateMake", () => ({
  useCreateMake: () => ({ mutateAsync: mockCreateMakeAsync }),
}));

vi.mock("./MakeForm", () => ({
  default: ({
    make,
    onSubmit,
  }: {
    make: { name: string };
    onSubmit: (data: { name: string }) => void;
  }) => (
    <div data-testid="make-form">
      <span data-testid="form-name">{make.name}</span>
      <button
        data-testid="form-submit"
        onClick={() => onSubmit({ name: "NewMake" })}
      >
        Submit
      </button>
    </div>
  ),
}));

describe("CreateMakeDialog", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockCreateMakeAsync.mockResolvedValue(undefined);
  });

  it("renders trigger button", () => {
    render(<CreateMakeDialog />);

    expect(
      screen.getByRole("button", { name: "makes.addMake" })
    ).toBeInTheDocument();
  });

  it("opens dialog when trigger is clicked", () => {
    render(<CreateMakeDialog />);

    fireEvent.click(screen.getByRole("button", { name: "makes.addMake" }));

    expect(screen.getByText("makes.createNewMake")).toBeInTheDocument();
  });

  it("renders MakeForm with empty name inside dialog", () => {
    render(<CreateMakeDialog />);

    fireEvent.click(screen.getByRole("button", { name: "makes.addMake" }));

    expect(screen.getByTestId("make-form")).toBeInTheDocument();
    expect(screen.getByTestId("form-name")).toHaveTextContent("");
  });

  it("calls createMakeAsync on form submit", async () => {
    render(<CreateMakeDialog />);

    fireEvent.click(screen.getByRole("button", { name: "makes.addMake" }));
    fireEvent.click(screen.getByTestId("form-submit"));

    expect(mockCreateMakeAsync).toHaveBeenCalledWith({ name: "NewMake" });
  });
});
