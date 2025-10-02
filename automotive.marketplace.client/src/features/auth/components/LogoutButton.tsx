import { Button } from "@/components/ui/button";
import { useAppDispatch } from "@/hooks/redux";
import { useNavigate } from "@tanstack/react-router";
import { LogOut } from "lucide-react";
import { useLogoutUser } from "../api/useLogoutUser";
import { clearCredentials } from "../state/authSlice";

const LogoutButton = () => {
  const dispatch = useAppDispatch();
  const { mutateAsync: logoutUserAsync } = useLogoutUser();
  const navigate = useNavigate();

  const handleLogout = async () => {
    await logoutUserAsync();
    dispatch(clearCredentials());
    await navigate({ to: "/login" });
  };

  return (
    <Button variant="secondary" onClick={() => void handleLogout()}>
      <LogOut />
    </Button>
  );
};

export default LogoutButton;
