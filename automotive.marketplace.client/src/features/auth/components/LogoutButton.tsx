import { useLogoutUser } from "@/features/auth/api/useLogoutUser";
import { clearCredentials } from "@/features/auth/state/authSlice";
import { useAppDispatch } from "@/shared/hooks/redux";
import { Button } from "../ui/button";

const LogoutButton = () => {
  const dispatch = useAppDispatch();
  const { mutateAsync: logoutUserAsync } = useLogoutUser();

  const handleLogout = async () => {
    await logoutUserAsync();
    dispatch(clearCredentials());
  };

  return (
    <Button variant="secondary" onClick={() => void handleLogout()}>
      Logout
    </Button>
  );
};

export default LogoutButton;
