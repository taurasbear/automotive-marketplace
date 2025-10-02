import { Button } from "@/components/ui/button";
import { useNavigate } from "@tanstack/react-router";

const RegisterButton = () => {
  const navigate = useNavigate();

  const handleRegister = async () => {
    await navigate({ to: "/register" });
  };

  return (
    <Button variant="secondary" onClick={() => void handleRegister()}>
      Sign up
    </Button>
  );
};

export default RegisterButton;
