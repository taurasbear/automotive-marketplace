import { Button } from "@/components/ui/button";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import { LoginSchema, setCredentials, useLoginUser } from "@/features/auth";
import { useAppDispatch } from "@/hooks/redux";
import { zodResolver } from "@hookform/resolvers/zod";
import { useNavigate } from "@tanstack/react-router";
import { useForm } from "react-hook-form";
import { useTranslation } from "react-i18next";
import { z } from "zod";

const Login = () => {
  const { t } = useTranslation("auth");
  const { mutateAsync: loginUserAsync } = useLoginUser();
  const navigate = useNavigate();

  const dispatch = useAppDispatch();

  const form = useForm({
    resolver: zodResolver(LoginSchema),
    defaultValues: {
      email: "",
      password: "",
    },
  });

  const onSubmit = async (formData: z.infer<typeof LoginSchema>) => {
    const { data: user } = await loginUserAsync({
      email: formData.email,
      password: formData.password,
    });

    dispatch(
      setCredentials({
        accessToken: user.accessToken,
        permissions: user.permissions,
        userId: user.userId,
      }),
    );

    await navigate({ to: "/" });
  };

  return (
    <div className="flex min-h-screen flex-col items-center justify-center">
      <h2 className="mb-6 text-2xl font-bold">{t("login.title")}</h2>
      <Form {...form}>
        <form
          onSubmit={form.handleSubmit(onSubmit)}
          className="w-full max-w-md space-y-6"
        >
          <FormField
            control={form.control}
            name="email"
            render={({ field }) => (
              <FormItem>
                <FormLabel>{t("login.fields.email")}</FormLabel>
                <FormControl>
                  <Input
                    type="email"
                    placeholder={t("login.fields.emailPlaceholder")}
                    {...field}
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            control={form.control}
            name="password"
            render={({ field }) => (
              <FormItem>
                <FormLabel>{t("login.fields.password")}</FormLabel>
                <FormControl>
                  <Input type="password" placeholder={t("login.fields.passwordPlaceholder")} {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <Button type="submit" className="w-full">
            {t("login.submit")}
          </Button>
        </form>
      </Form>
    </div>
  );
};

export default Login;
