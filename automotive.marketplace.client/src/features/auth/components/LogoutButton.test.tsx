import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, it, expect, vi, beforeEach } from "vitest";
import LogoutButton from "./LogoutButton";

const { mockLogoutAsync, mockDispatch, mockNavigate } = vi.hoisted(() => ({
  mockLogoutAsync: vi.fn(),
  mockDispatch: vi.fn(),
  mockNavigate: vi.fn(),
}));

vi.mock("../api/useLogoutUser", () => ({
  useLogoutUser: () => ({ mutateAsync: mockLogoutAsync }),
}));

vi.mock("@/hooks/redux", () => ({
  useAppDispatch: () => mockDispatch,
}));

vi.mock("@tanstack/react-router", () => ({
  useNavigate: () => mockNavigate,
}));

beforeEach(() => {
  mockLogoutAsync.mockReset().mockResolvedValue({});
  mockDispatch.mockReset();
  mockNavigate.mockReset();
});

describe("LogoutButton", () => {
  it("renders a button", () => {
    render(<LogoutButton />);
    expect(screen.getByRole("button")).toBeInTheDocument();
  });

  it("calls logoutUserAsync, dispatches clearCredentials, and navigates to login on click", async () => {
    const user = userEvent.setup();
    render(<LogoutButton />);

    await user.click(screen.getByRole("button"));

    await waitFor(() => {
      expect(mockLogoutAsync).toHaveBeenCalledTimes(1);
    });
    expect(mockDispatch).toHaveBeenCalledWith(
      expect.objectContaining({ type: "auth/clearCredentials" }),
    );
    expect(mockNavigate).toHaveBeenCalledWith({ to: "/login" });
  });
});
