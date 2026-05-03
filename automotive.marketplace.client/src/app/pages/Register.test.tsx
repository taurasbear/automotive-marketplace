import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, it, expect, vi, beforeEach } from "vitest";
import { z } from "zod";
import Register from "./Register";

const { mockRegisterAsync, mockDispatch, mockNavigate } = vi.hoisted(() => ({
  mockRegisterAsync: vi.fn(),
  mockDispatch: vi.fn(),
  mockNavigate: vi.fn(),
}));

vi.mock("@/features/auth", () => ({
  useRegisterUser: () => ({ mutateAsync: mockRegisterAsync }),
  RegisterSchema: z.object({
    username: z.string().min(3),
    email: z.string().email(),
    password: z.string().min(6),
  }),
  setCredentials: (payload: Record<string, unknown>) => ({
    type: "auth/setCredentials",
    payload,
  }),
}));

vi.mock("@/hooks/redux", () => ({
  useAppDispatch: () => mockDispatch,
  useAppSelector: vi.fn(),
}));

vi.mock("@tanstack/react-router", () => ({
  useNavigate: () => mockNavigate,
  Link: ({ children, to }: { children: React.ReactNode; to?: string }) => (
    <a href={String(to ?? "")}>{children}</a>
  ),
}));

vi.mock("react-i18next", () => ({
  useTranslation: () => ({
    t: (key: string) => key,
  }),
}));

beforeEach(() => {
  mockRegisterAsync.mockReset();
  mockDispatch.mockReset();
  mockNavigate.mockReset();
});

describe("Register page", () => {
  it("renders username, email, and password fields and a submit button", () => {
    render(<Register />);
    expect(screen.getByPlaceholderText("register.fields.usernamePlaceholder")).toBeInTheDocument();
    expect(screen.getByPlaceholderText("register.fields.emailPlaceholder")).toBeInTheDocument();
    expect(screen.getByPlaceholderText("register.fields.passwordPlaceholder")).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "register.submit" })).toBeInTheDocument();
  });

  it("toggles password visibility when the eye icon is clicked", async () => {
    const user = userEvent.setup();
    render(<Register />);

    const passwordInput = screen.getByPlaceholderText("register.fields.passwordPlaceholder");
    expect(passwordInput).toHaveAttribute("type", "password");

    await user.click(screen.getByLabelText("register.fields.showPassword"));
    expect(passwordInput).toHaveAttribute("type", "text");

    await user.click(screen.getByLabelText("register.fields.hidePassword"));
    expect(passwordInput).toHaveAttribute("type", "password");
  });

  it("renders a link to the login page", () => {
    render(<Register />);
    const loginLink = screen.getByRole("link");
    expect(loginLink).toHaveAttribute("href", "/login");
  });

  it("calls registerUserAsync, dispatches setCredentials, and navigates on successful submit", async () => {
    mockRegisterAsync.mockResolvedValue({
      data: { accessToken: "tok-456", userId: "user-2" },
    });

    const user = userEvent.setup();
    render(<Register />);

    await user.type(screen.getByPlaceholderText("register.fields.usernamePlaceholder"), "testuser");
    await user.type(screen.getByPlaceholderText("register.fields.emailPlaceholder"), "test@example.com");
    await user.type(screen.getByPlaceholderText("register.fields.passwordPlaceholder"), "Password123!");
    await user.click(screen.getByRole("button", { name: "register.submit" }));

    await waitFor(() => {
      expect(mockRegisterAsync).toHaveBeenCalledWith({
        username: "testuser",
        email: "test@example.com",
        password: "Password123!",
      });
    });

    expect(mockDispatch).toHaveBeenCalledWith(
      expect.objectContaining({
        payload: { accessToken: "tok-456", permissions: [], userId: "user-2" },
      }),
    );
    expect(mockNavigate).toHaveBeenCalledWith({ to: "/" });
  });
});
