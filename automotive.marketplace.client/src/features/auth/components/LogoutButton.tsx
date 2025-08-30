import { Button } from "@/components/ui/button";
import { useAppDispatch } from "@/hooks/redux";
import { useLogoutUser } from "../api/useLogoutUser";
import { clearCredentials } from "../state/authSlice";

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
