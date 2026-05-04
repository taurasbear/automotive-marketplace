import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, it, expect, vi } from "vitest";
import RegisterButton from "./RegisterButton";

const { mockNavigate } = vi.hoisted(() => ({
  mockNavigate: vi.fn(),
}));

vi.mock("@tanstack/react-router", () => ({
  useNavigate: () => mockNavigate,
}));

vi.mock("react-i18next", () => ({
  useTranslation: () => ({
    t: (key: string) => key,
  }),
}));

describe("RegisterButton", () => {
  it("renders a button with the sign up text", () => {
    render(<RegisterButton />);
    expect(screen.getByRole("button", { name: "header.signUp" })).toBeInTheDocument();
  });

  it("navigates to /register on click", async () => {
    const user = userEvent.setup();
    render(<RegisterButton />);

    await user.click(screen.getByRole("button", { name: "header.signUp" }));

    await waitFor(() => {
      expect(mockNavigate).toHaveBeenCalledWith({ to: "/register" });
    });
  });
});
