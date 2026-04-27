import { useState } from "react";
import { Button } from "@/components/ui/button";
import {
  Form,
  FormControl,
  FormDescription,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import { Eye, EyeOff } from "lucide-react";
import {
  RegisterSchema,
  setCredentials,
  useRegisterUser,
} from "@/features/auth";
import { useAppDispatch } from "@/hooks/redux";
import { zodResolver } from "@hookform/resolvers/zod";
import { Link, useNavigate } from "@tanstack/react-router";
import { useForm } from "react-hook-form";
import { useTranslation } from "react-i18next";
import { z } from "zod";

const Register = () => {
  const [showPassword, setShowPassword] = useState(false);
  const { t } = useTranslation("auth");
  const { mutateAsync: registerUserAsync } = useRegisterUser();
  const navigate = useNavigate();

  const dispatch = useAppDispatch();

  const form = useForm({
    resolver: zodResolver(RegisterSchema),
    defaultValues: {
      username: "",
      email: "",
      password: "",
    },
  });

  const onSubmit = async (formData: z.infer<typeof RegisterSchema>) => {
    const { data: user } = await registerUserAsync({
      username: formData.username,
      email: formData.email,
      password: formData.password,
    });

    dispatch(
      setCredentials({
        accessToken: user.accessToken,
        permissions: [],
        userId: user.userId,
      }),
    );

    await navigate({ to: "/" });
  };

  return (
    <div className="flex min-h-screen flex-col items-center justify-center">
      <h2 className="mb-6 text-2xl font-bold">{t("register.title")}</h2>
      <Form {...form}>
        <form
          onSubmit={form.handleSubmit(onSubmit)}
          className="w-full max-w-md space-y-6"
        >
          <FormField
            control={form.control}
            name="username"
            render={({ field }) => (
              <FormItem>
                <FormLabel>{t("register.fields.username")}</FormLabel>
                <FormControl>
                  <Input
                    placeholder={t("register.fields.usernamePlaceholder")}
                    {...field}
                  />
                </FormControl>
                <FormDescription>
                  {t("register.fields.usernameDescription")}
                </FormDescription>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            control={form.control}
            name="email"
            render={({ field }) => (
              <FormItem>
                <FormLabel>{t("register.fields.email")}</FormLabel>
                <FormControl>
                  <Input
                    type="email"
                    placeholder={t("register.fields.emailPlaceholder")}
                    {...field}
                  />
                </FormControl>
                <FormDescription>
                  {t("register.fields.emailDescription")}
                </FormDescription>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            control={form.control}
            name="password"
            render={({ field }) => (
              <FormItem>
                <FormLabel>{t("register.fields.password")}</FormLabel>
                <FormControl>
                  <div className="relative">
                    <Input
                      type={showPassword ? "text" : "password"}
                      placeholder={t("register.fields.passwordPlaceholder")}
                      {...field}
                    />
                    <button
                      type="button"
                      onClick={() => setShowPassword((p) => !p)}
                      aria-label={showPassword ? t("register.fields.hidePassword") : t("register.fields.showPassword")}
                      className="absolute right-3 top-1/2 -translate-y-1/2 text-muted-foreground hover:text-foreground"
                      tabIndex={-1}
                    >
                      {showPassword ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
                    </button>
                  </div>
                </FormControl>
                <FormDescription>
                  {t("register.fields.passwordDescription")}
                </FormDescription>
                <FormMessage />
              </FormItem>
            )}
          />
          <Button type="submit" className="w-full">
            {t("register.submit")}
          </Button>
        </form>
      </Form>
      <Link to="/login">
        <Button variant="link" className="m-4">
          {t("register.alreadyHaveAccount")}
        </Button>
      </Link>
    </div>
  );
};

export default Register;
